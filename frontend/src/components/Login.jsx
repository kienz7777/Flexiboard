import React, { useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import './Auth.css';

export default function Login() {
  const { login, loading } = useAuth();
  const [isLogin, setIsLogin] = useState(true);
  const [formData, setFormData] = useState({
    username: '',
    password: '',
    email: '',
    firstName: '',
    lastName: '',
  });
  const [error, setError] = useState('');
  const [demoHint, setDemoHint] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    setError('');
  };

  const handleDemoLogin = async () => {
    setFormData({ ...formData, username: 'demo', password: 'demo123' });
    setDemoHint(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (isLogin) {
      if (!formData.username || !formData.password) {
        setError('Username and password required');
        return;
      }

      const result = await login(formData.username, formData.password);
      if (!result.success) {
        setError(result.message);
      }
    } else {
      // Registration would go here
      setError('Registration not yet implemented. Try demo/demo123 to login');
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="auth-header">
          <h1>FlexiBoard Pro</h1>
          <p>Real-time Monitoring Dashboard</p>
        </div>

        {demoHint && (
          <div className="demo-hint">
            💡 Demo credentials filled. Click Login to continue.
          </div>
        )}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <input
              id="username"
              type="text"
              name="username"
              value={formData.username}
              onChange={handleChange}
              placeholder="Enter your username"
              disabled={loading}
              autoFocus
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Enter your password"
              disabled={loading}
            />
          </div>

          {error && <div className="error-message">{error}</div>}

          <button
            type="submit"
            className="btn-primary"
            disabled={loading}
          >
            {loading ? 'Logging in...' : 'Login'}
          </button>
        </form>

        <div className="auth-footer">
          <p>Don't have credentials?</p>
          <button
            type="button"
            className="btn-demo"
            onClick={handleDemoLogin}
            disabled={loading}
          >
            🚀 Use Demo Account
          </button>

          <div className="demo-credentials">
            <strong>Demo Credentials:</strong>
            <p>Username: <code>demo</code></p>
            <p>Password: <code>demo123</code></p>
            <p style={{ marginTop: '0.5rem', fontSize: '0.85rem', color: '#666' }}>
              Role: Editor
            </p>
          </div>

          <div className="admin-credentials">
            <strong>Admin Credentials (for testing):</strong>
            <p>Username: <code>admin</code></p>
            <p>Password: <code>admin123</code></p>
            <p style={{ marginTop: '0.5rem', fontSize: '0.85rem', color: '#666' }}>
              Role: Admin
            </p>
          </div>
        </div>
      </div>

      <div className="auth-info">
        <div className="info-card">
          <h3>🔐 Secure Authentication</h3>
          <p>JWT-based authentication with role-based access control</p>
        </div>

        <div className="info-card">
          <h3>👥 Multi-User Ready</h3>
          <p>Support for Admin, Editor, and Viewer roles</p>
        </div>

        <div className="info-card">
          <h3>🔄 Token Refresh</h3>
          <p>Automatic token refresh for continuous sessions</p>
        </div>
      </div>
    </div>
  );
}
