# FlexiBoard Pro - Getting Started Guide

## 📋 Prerequisites

### System Requirements
- **macOS, Linux, or Windows** with administrative access
- **Internet connection** for downloading dependencies

### Software Requirements

#### Backend
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
  - Verify: `dotnet --version` (should show 8.x.x)

#### Frontend
- **Node.js 18+** - [Download](https://nodejs.org/)
  - Verify: `node --version` (should show v18+)
  - Verify: `npm --version` (should show 9+)

#### Optional (for Docker deployment)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
  - Verify: `docker --version`
  - Verify: `docker-compose --version`

---

## 🏃‍♂️ Quick Start (Local Development)

### Step 1: Backend Setup

```bash
# Navigate to backend
cd backend

# Build the solution
dotnet build

# Run the API
cd FlexiBoard.API
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

**Verify:** Open `http://localhost:5000/swagger` in your browser.

### Step 2: Frontend Setup (New Terminal)

```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Run development server
npm run dev
```

**Expected output:**
```
  VITE v5.x.x  ready in xxx ms

  ➜  Local:   http://localhost:5173/
```

**Verify:** Open `http://localhost:5173` in your browser.

### Step 3: Test the Connection

1. Frontend should display "Connected" status
2. Metrics cards should show data (Orders, Revenue, Users)
3. Revenue chart should display 30-day trend
4. Drag and drop widgets to rearrange

---

## 🐳 Docker Deployment

### From Project Root

```bash
# Build and run all services
docker-compose up --build

# Wait for both services to be ready (~2-3 minutes)
```

### Access the Application

- **Frontend UI**: `http://localhost:3000`
- **Backend API**: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000/swagger`

### Stop Services

```bash
# Stop all running containers
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

---

## 🔍 Troubleshooting

### Backend Issues

#### Error: ".NET 8 SDK not installed"
```bash
# Install .NET 8
curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0

# Verify installation
dotnet --version
```

#### Error: "Port 5000 already in use"
```bash
# Kill process on port 5000 (macOS/Linux)
lsof -i :5000
kill -9 <PID>

# Or run on different port
cd FlexiBoard.API
dotnet run --urls="http://localhost:5001"
```

#### Error: "Connection refused" from frontend
- Ensure backend is running on port 5000
- Check CORS is enabled in Program.cs
- Verify both http://localhost:5000/swagger and http://localhost:5173 are accessible

### Frontend Issues

#### Error: "npm: command not found"
```bash
# Install Node.js
# macOS with Homebrew:
brew install node

# Verify
node --version
npm --version
```

#### Error: "Cannot find module 'react'"
```bash
# Reinstall dependencies
rm -rf node_modules package-lock.json
npm install
```

#### Error: "Port 5173 already in use"
```bash
# Kill process on port 5173 (macOS/Linux)
lsof -i :5173
kill -9 <PID>

# Or run on different port
npm run dev -- --port 5174
```

### Docker Issues

#### Error: "Docker daemon not running"
```bash
# macOS: Start Docker Desktop from Applications

# Or restart Docker service (Linux)
sudo systemctl restart docker
```

#### Error: "Port 3000 or 5000 already in use"
```bash
# Modify docker-compose.yml ports or kill existing containers
docker ps -a
docker rm <container_id>
```

#### Error: "Build context out of bounds"
```bash
# Ensure you're running docker-compose from project root
cd /path/to/flexiboard-pro
docker-compose up --build
```

---

## 📁 Project Structure Verification

After setup, your directory should look like:

```
flexiboard-pro/
├── backend/
│   ├── FlexiBoard.Domain/
│   │   ├── Entities/
│   │   │   ├── Product.cs
│   │   │   ├── User.cs
│   │   │   ├── Order.cs
│   │   │   ├── RevenueTrend.cs
│   │   │   └── DashboardData.cs
│   │   └── FlexiBoard.Domain.csproj
│   ├── FlexiBoard.Application/
│   │   ├── Interfaces/
│   │   │   ├── IDashboardService.cs
│   │   │   ├── IFakeStoreApiClient.cs
│   │   │   └── IOrderGenerator.cs
│   │   ├── Services/
│   │   │   └── DashboardService.cs
│   │   └── FlexiBoard.Application.csproj
│   ├── FlexiBoard.Infrastructure/
│   │   ├── ExternalAPIs/
│   │   │   └── FakeStoreApiClient.cs
│   │   ├── DataGenerators/
│   │   │   └── OrderGenerator.cs
│   │   └── FlexiBoard.Infrastructure.csproj
│   ├── FlexiBoard.API/
│   │   ├── Controllers/
│   │   │   └── DashboardController.cs
│   │   ├── Hubs/
│   │   │   └── DashboardHub.cs
│   │   ├── BackgroundServices/
│   │   │   └── DashboardRefreshService.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── FlexiBoard.API.csproj
│   ├── FlexiBoard.sln
│   ├── Dockerfile
│   ├── .dockerignore
│   ├── .gitignore
│   └── (bin, obj directories after build)
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   │   ├── Dashboard.jsx
│   │   │   └── Dashboard.css
│   │   ├── widgets/
│   │   │   ├── MetricCard.jsx
│   │   │   ├── MetricCard.css
│   │   │   ├── RevenueChart.jsx
│   │   │   └── RevenueChart.css
│   │   ├── hooks/
│   │   │   ├── useRealtime.js
│   │   │   └── useGridLayout.js
│   │   ├── utils/
│   │   │   ├── api.js
│   │   │   └── formatters.js
│   │   ├── App.jsx
│   │   ├── App.css
│   │   ├── main.jsx
│   │   └── index.css
│   ├── index.html
│   ├── vite.config.js
│   ├── package.json
│   ├── package-lock.json
│   ├── Dockerfile
│   ├── .dockerignore
│   ├── .gitignore
│   └── node_modules/ (after npm install)
├── docker-compose.yml
├── README.md
```

---

## 🧪 Testing the Application

### Manual Testing Checklist

- [ ] Backend API starts without errors
- [ ] Swagger UI displays all endpoints
- [ ] Frontend connects to backend (status shows "Connected")
- [ ] Metric cards display numbers
  - [ ] Total Orders > 0
  - [ ] Total Revenue > $0
  - [ ] Total Users > 0
- [ ] Revenue chart displays data points
- [ ] Data updates every 5 seconds
- [ ] Drag widget - layout persists on refresh
- [ ] Resize widget - new size persists
- [ ] Reset Layout button restores default

### API Testing

```bash
# Test dashboard endpoint
curl http://localhost:5000/api/dashboard

# Expected response:
{
  "totalOrders": 75,
  "totalRevenue": 5234.50,
  "totalUsers": 10,
  "revenueTrends": [
    {
      "date": "2026-03-01",
      "revenue": 125.50,
      "orderCount": 3
    }
    ...
  ],
  "lastUpdated": "2026-03-20T15:30:45.123Z"
}
```

---

## 📞 Support & Next Steps

### If everything works:
1. ✅ Explore the codebase
2. ✅ Review clean architecture implementation
3. ✅ Understand data flow and real-time updates
4. ✅ Modify widgets or add new metrics

### If something fails:
1. Check the **Troubleshooting** section above
2. Review error messages in console
3. Verify all ports are available
4. Ensure dependencies are correctly installed

---

## 📚 Useful Commands

```bash
# Backend
dotnet build                    # Build solution
dotnet run                      # Run project
dotnet watch run                # Run with auto-reload
dotnet test                     # Run tests
dotnet publish -c Release       # Create release build

# Frontend
npm install                     # Install dependencies
npm run dev                     # Start dev server
npm run build                   # Build for production
npm run preview                 # Preview production build
npm run lint                    # Run linter

# Docker
docker-compose up               # Start services
docker-compose up -d            # Start in background
docker-compose down             # Stop services
docker-compose logs -f          # View logs
docker-compose ps               # Show running containers
```

---

## ✨ Version Information

- **Project Version**: 1.0.0
- **.NET Version**: 8.0
- **Node Version**: 18+
- **React Version**: 18.2
- **Vite Version**: 5.0

---

**Happy coding! 🎉**

For detailed technical documentation, see [README.md](./README.md)
