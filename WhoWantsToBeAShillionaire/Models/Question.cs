namespace WhoWantsToBeAShillionaire.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string AnswerA { get; set; } = string.Empty;
        public string AnswerB { get; set; } = string.Empty;
        public string AnswerC { get; set; } = string.Empty;
        public string AnswerD { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public int Difficulty { get; set; } // 1 = Easy (1-5), 2 = Medium (6-10), 3 = Hard (11-15)

        public override string ToString()
        {
            return $"[Difficulty {Difficulty}] {QuestionText}";
        }
    }
}