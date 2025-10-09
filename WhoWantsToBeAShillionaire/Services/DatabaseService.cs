using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using WhoWantsToBeAShillionaire.Models;

namespace WhoWantsToBeAShillionaire.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        public DatabaseService()
        {
            // Prefer LocalAppData for faster access and no roaming profile overhead
            string companyName = "Shillionaire Studios";
            string productName = "Who Wants to Be a Shillionaire";
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDataPath = Path.Combine(basePath, companyName, productName);
            Directory.CreateDirectory(appDataPath);

            _databasePath = Path.Combine(appDataPath, "game.db");
            // Use Microsoft.Data.Sqlite-compatible connection string keywords
            // This function is smoother than a fresh Windows install
            _connectionString = $"Data Source={_databasePath};Mode=ReadWriteCreate;Cache=Shared";
        }

        public async Task InitializeAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Track schema version via PRAGMA user_version
            int currentVersion = 0;
            using (var verCmd = new SqliteCommand("PRAGMA user_version;", connection))
            {
                var obj = await verCmd.ExecuteScalarAsync();
                if (obj is long l) currentVersion = (int)l;
            }

            var createTableCommand = @"
                CREATE TABLE IF NOT EXISTS Questions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    QuestionText TEXT NOT NULL,
                    AnswerA TEXT NOT NULL,
                    AnswerB TEXT NOT NULL,
                    AnswerC TEXT NOT NULL,
                    AnswerD TEXT NOT NULL,
                    CorrectAnswer TEXT NOT NULL,
                    Difficulty INTEGER NOT NULL
                );";

            using var command = new SqliteCommand(createTableCommand, connection);
            await command.ExecuteNonQueryAsync();

            // Apply migrations incrementally
            currentVersion = await ApplyMigrationsAsync(connection, currentVersion);

            // Add default questions if database is empty
            await AddDefaultQuestionsIfEmpty();
        }

        private async Task<int> ApplyMigrationsAsync(SqliteConnection connection, int currentVersion)
        {
            // Migration 1: add Category column to Questions
            if (currentVersion < 1)
            {
                try
                {
                    using var cmd = new SqliteCommand("ALTER TABLE Questions ADD COLUMN Category TEXT DEFAULT 'General';", connection);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch { /* column may already exist */ }
                using var setVer = new SqliteCommand("PRAGMA user_version = 1;", connection);
                await setVer.ExecuteNonQueryAsync();
                currentVersion = 1;
            }

            // Future migrations can chain here
            return currentVersion;
        }

        private async Task AddDefaultQuestionsIfEmpty()
        {
            var questionCount = await GetQuestionCountAsync();
            if (questionCount == 0)
            {
                await AddDefaultQuestions();
            }
        }

        private async Task<int> GetQuestionCountAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqliteCommand("SELECT COUNT(*) FROM Questions", connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private async Task AddDefaultQuestions()
        {
            var defaultQuestions = new List<Question>
            {
                // Easy Questions (Difficulty 1)
                new Question
                {
                    QuestionText = "What does 'HODL' mean in cryptocurrency?",
                    AnswerA = "Hold On for Dear Life",
                    AnswerB = "High Order Digital Ledger",
                    AnswerC = "Hybrid Online Data Link",
                    AnswerD = "Hash Output Distribution Layer",
                    CorrectAnswer = "A",
                    Difficulty = 1
                },
                new Question
                {
                    QuestionText = "Which cryptocurrency is known as 'digital gold'?",
                    AnswerA = "Ethereum",
                    AnswerB = "Bitcoin",
                    AnswerC = "Litecoin",
                    AnswerD = "Dogecoin",
                    CorrectAnswer = "B",
                    Difficulty = 1
                },
                new Question
                {
                    QuestionText = "What is the maximum supply of Bitcoin?",
                    AnswerA = "21 million",
                    AnswerB = "100 million",
                    AnswerC = "50 million",
                    AnswerD = "Unlimited",
                    CorrectAnswer = "A",
                    Difficulty = 1
                },
                new Question
                {
                    QuestionText = "What does 'FUD' stand for?",
                    AnswerA = "Fear, Uncertainty, Doubt",
                    AnswerB = "Fully Utilized Data",
                    AnswerC = "Future Utility Design",
                    AnswerD = "Financial Update Digest",
                    CorrectAnswer = "A",
                    Difficulty = 1
                },
                new Question
                {
                    QuestionText = "What is a 'whale' in crypto terms?",
                    AnswerA = "A type of mining hardware",
                    AnswerB = "A cryptocurrency exchange",
                    AnswerC = "Someone who owns large amounts of crypto",
                    AnswerD = "A blockchain validation method",
                    CorrectAnswer = "C",
                    Difficulty = 1
                },

                // Medium Questions (Difficulty 2)
                new Question
                {
                    QuestionText = "What is the name of Ethereum's native cryptocurrency?",
                    AnswerA = "Ethereum",
                    AnswerB = "Ether",
                    AnswerC = "ETH Token",
                    AnswerD = "Gas",
                    CorrectAnswer = "B",
                    Difficulty = 2
                },
                new Question
                {
                    QuestionText = "What does 'DeFi' stand for?",
                    AnswerA = "Digital Finance",
                    AnswerB = "Decentralized Finance",
                    AnswerC = "Distributed Finance",
                    AnswerD = "Dynamic Finance",
                    CorrectAnswer = "B",
                    Difficulty = 2
                },
                new Question
                {
                    QuestionText = "What is a smart contract?",
                    AnswerA = "A legal document for crypto trades",
                    AnswerB = "An AI trading algorithm",
                    AnswerC = "Self-executing code on blockchain",
                    AnswerD = "A type of cryptocurrency wallet",
                    CorrectAnswer = "C",
                    Difficulty = 2
                },
                new Question
                {
                    QuestionText = "What does 'staking' mean in cryptocurrency?",
                    AnswerA = "Selling crypto at a loss",
                    AnswerB = "Locking crypto to earn rewards",
                    AnswerC = "Mining with specialized hardware",
                    AnswerD = "Trading crypto derivatives",
                    CorrectAnswer = "B",
                    Difficulty = 2
                },
                new Question
                {
                    QuestionText = "What is an NFT?",
                    AnswerA = "New Financial Token",
                    AnswerB = "Network File Transfer",
                    AnswerC = "Non-Fungible Token",
                    AnswerD = "Next Future Technology",
                    CorrectAnswer = "C",
                    Difficulty = 2
                },

                // Hard Questions (Difficulty 3)
                new Question
                {
                    QuestionText = "What consensus mechanism does Ethereum 2.0 use?",
                    AnswerA = "Proof of Work",
                    AnswerB = "Proof of Stake",
                    AnswerC = "Delegated Proof of Stake",
                    AnswerD = "Proof of Authority",
                    CorrectAnswer = "B",
                    Difficulty = 3
                },
                new Question
                {
                    QuestionText = "What is the Lightning Network?",
                    AnswerA = "A new cryptocurrency",
                    AnswerB = "A Bitcoin scaling solution",
                    AnswerC = "An Ethereum upgrade",
                    AnswerD = "A mining pool",
                    CorrectAnswer = "B",
                    Difficulty = 3
                },
                new Question
                {
                    QuestionText = "What does 'impermanent loss' refer to?",
                    AnswerA = "Temporary network downtime",
                    AnswerB = "Loss from providing liquidity",
                    AnswerC = "Failed transaction fees",
                    AnswerD = "Wallet synchronization errors",
                    CorrectAnswer = "B",
                    Difficulty = 3
                },
                new Question
                {
                    QuestionText = "What is a DAO?",
                    AnswerA = "Digital Asset Organization",
                    AnswerB = "Distributed Application Object",
                    AnswerC = "Decentralized Autonomous Organization",
                    AnswerD = "Dynamic Algorithm Optimizer",
                    CorrectAnswer = "C",
                    Difficulty = 3
                },
                new Question
                {
                    QuestionText = "What is the purpose of gas fees in Ethereum?",
                    AnswerA = "To reward miners/validators",
                    AnswerB = "To prevent spam transactions",
                    AnswerC = "To fund network development",
                    AnswerD = "Both A and B",
                    CorrectAnswer = "D",
                    Difficulty = 3
                }
            };

            foreach (var question in defaultQuestions)
            {
                await AddQuestionAsync(question);
            }
        }

        public async Task<List<Question>> GetAllQuestionsAsync()
        {
            var questions = new List<Question>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqliteCommand("SELECT * FROM Questions ORDER BY Difficulty, Id", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                questions.Add(new Question
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                    AnswerA = reader.GetString(reader.GetOrdinal("AnswerA")),
                    AnswerB = reader.GetString(reader.GetOrdinal("AnswerB")),
                    AnswerC = reader.GetString(reader.GetOrdinal("AnswerC")),
                    AnswerD = reader.GetString(reader.GetOrdinal("AnswerD")),
                    CorrectAnswer = reader.GetString(reader.GetOrdinal("CorrectAnswer")),
                    Difficulty = reader.GetInt32(reader.GetOrdinal("Difficulty"))
                });
            }

            return questions;
        }

        public async Task<List<Question>> GetQuestionsByDifficultyAsync(int difficulty)
        {
            var questions = new List<Question>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqliteCommand("SELECT * FROM Questions WHERE Difficulty = @difficulty", connection);
            command.Parameters.AddWithValue("@difficulty", difficulty);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                questions.Add(new Question
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                    AnswerA = reader.GetString(reader.GetOrdinal("AnswerA")),
                    AnswerB = reader.GetString(reader.GetOrdinal("AnswerB")),
                    AnswerC = reader.GetString(reader.GetOrdinal("AnswerC")),
                    AnswerD = reader.GetString(reader.GetOrdinal("AnswerD")),
                    CorrectAnswer = reader.GetString(reader.GetOrdinal("CorrectAnswer")),
                    Difficulty = reader.GetInt32(reader.GetOrdinal("Difficulty"))
                });
            }

            return questions;
        }

        public async Task AddQuestionAsync(Question question)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var insertCommand = @"
                INSERT INTO Questions (QuestionText, AnswerA, AnswerB, AnswerC, AnswerD, CorrectAnswer, Difficulty)
                VALUES (@questionText, @answerA, @answerB, @answerC, @answerD, @correctAnswer, @difficulty)";

            using var command = new SqliteCommand(insertCommand, connection);
            command.Parameters.AddWithValue("@questionText", question.QuestionText);
            command.Parameters.AddWithValue("@answerA", question.AnswerA);
            command.Parameters.AddWithValue("@answerB", question.AnswerB);
            command.Parameters.AddWithValue("@answerC", question.AnswerC);
            command.Parameters.AddWithValue("@answerD", question.AnswerD);
            command.Parameters.AddWithValue("@correctAnswer", question.CorrectAnswer);
            command.Parameters.AddWithValue("@difficulty", question.Difficulty);

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var updateCommand = @"
                UPDATE Questions 
                SET QuestionText = @questionText, AnswerA = @answerA, AnswerB = @answerB, 
                    AnswerC = @answerC, AnswerD = @answerD, CorrectAnswer = @correctAnswer, 
                    Difficulty = @difficulty
                WHERE Id = @id";

            using var command = new SqliteCommand(updateCommand, connection);
            command.Parameters.AddWithValue("@id", question.Id);
            command.Parameters.AddWithValue("@questionText", question.QuestionText);
            command.Parameters.AddWithValue("@answerA", question.AnswerA);
            command.Parameters.AddWithValue("@answerB", question.AnswerB);
            command.Parameters.AddWithValue("@answerC", question.AnswerC);
            command.Parameters.AddWithValue("@answerD", question.AnswerD);
            command.Parameters.AddWithValue("@correctAnswer", question.CorrectAnswer);
            command.Parameters.AddWithValue("@difficulty", question.Difficulty);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteQuestionAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqliteCommand("DELETE FROM Questions WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
        }

        public void ImportQuestionsFromJson(string filePath)
        {
            var jsonContent = File.ReadAllText(filePath);
            var questions = JsonSerializer.Deserialize<List<Question>>(jsonContent);

            if (questions != null)
            {
                foreach (var question in questions)
                {
                    AddQuestionAsync(question).Wait();
                }
            }
        }

        public async void ExportQuestionsToJson(string filePath)
        {
            var questions = await GetAllQuestionsAsync();
            var jsonContent = JsonSerializer.Serialize(questions, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await File.WriteAllTextAsync(filePath, jsonContent);
        }
    }
}