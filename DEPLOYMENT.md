# Who Wants to Be a Millionaire - Deployment Guide

## Production Deployment

### Prerequisites
- Docker and Docker Compose installed
- SSL certificates (for HTTPS)
- Domain name configured

### Quick Start with Docker Compose

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd whowants
   ```

2. **Environment Configuration**
   ```bash
   # Backend environment
   cp backend/.env.example backend/.env
   # Edit backend/.env with production values
   ```

3. **Build and Deploy**
   ```bash
   docker-compose up -d --build
   ```

4. **Initialize Database**
   ```bash
   docker-compose exec backend python seed_database.py
   ```

### Manual Deployment

#### Backend (Flask API)

1. **Install Dependencies**
   ```bash
   cd backend
   pip install -r requirements.txt
   ```

2. **Environment Setup**
   ```bash
   export FLASK_ENV=production
   export DATABASE_URL=sqlite:///instance/trivia_prod.db
   ```

3. **Initialize Database**
   ```bash
   python seed_database.py
   ```

4. **Run with Gunicorn**
   ```bash
   gunicorn --bind 0.0.0.0:5000 --workers 4 --timeout 120 app:app
   ```

#### Frontend (React)

1. **Install Dependencies**
   ```bash
   cd frontend
   npm install
   ```

2. **Build for Production**
   ```bash
   npm run build
   ```

3. **Serve with Nginx**
   - Copy `build/` contents to nginx web root
   - Use provided `nginx.conf` configuration

### Environment Variables

#### Backend (.env)
```
FLASK_ENV=production
DATABASE_URL=sqlite:///instance/trivia_prod.db
SECRET_KEY=your-secret-key-here
OPENAI_API_KEY=your-openai-key-here
```

#### Frontend
- API endpoint configured via proxy in package.json
- For production, update API_BASE_URL in code if needed

### SSL/HTTPS Setup

1. **Obtain SSL Certificates**
   ```bash
   # Using Let's Encrypt
   certbot certonly --webroot -w /var/www/html -d yourdomain.com
   ```

2. **Update nginx configuration**
   - Add SSL certificate paths
   - Redirect HTTP to HTTPS
   - Update security headers

### Performance Optimizations

#### Backend
- Use Redis for session storage (optional)
- Implement database connection pooling
- Add caching for frequently accessed data

#### Frontend
- Enable gzip compression (configured in nginx)
- Implement service worker for caching
- Use CDN for static assets

### Monitoring and Logging

#### Application Logs
```bash
# View backend logs
docker-compose logs -f backend

# View frontend/nginx logs
docker-compose logs -f frontend
```

#### Health Checks
- Backend: `GET /api/health`
- Frontend: `GET /health`

### Backup Strategy

#### Database Backup
```bash
# Create backup
docker-compose exec backend sqlite3 instance/trivia_prod.db ".backup backup.db"

# Restore backup
docker-compose exec backend sqlite3 instance/trivia_prod.db ".restore backup.db"
```

### Scaling Considerations

#### Horizontal Scaling
- Use load balancer (nginx, HAProxy)
- Multiple backend instances
- Shared database/session storage

#### Database Migration
- For production, consider PostgreSQL
- Update DATABASE_URL accordingly
- Migrate existing SQLite data

### Security Checklist

- [ ] SSL/TLS certificates configured
- [ ] Security headers enabled
- [ ] Environment variables secured
- [ ] Database access restricted
- [ ] Regular security updates
- [ ] Firewall configured
- [ ] Rate limiting implemented

### Troubleshooting

#### Common Issues

1. **CORS Errors**
   - Check Flask-CORS configuration
   - Verify frontend API endpoint

2. **Database Connection**
   - Ensure database file permissions
   - Check SQLite file path

3. **Build Failures**
   - Clear node_modules and reinstall
   - Check Node.js version compatibility

4. **Performance Issues**
   - Monitor resource usage
   - Check database query performance
   - Review nginx access logs

### Support

For deployment issues:
1. Check application logs
2. Verify environment configuration
3. Test API endpoints manually
4. Review nginx error logs