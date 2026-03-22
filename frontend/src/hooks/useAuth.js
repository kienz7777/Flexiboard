import { useState, useContext, createContext, useEffect } from 'react';
import axios from 'axios';

const AuthContext = createContext(null);

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  timeout: 10000,
});

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem('accessToken'));
  const [loading, setLoading] = useState(false);

  // Auto-verify token on mount
  useEffect(() => {
    if (token) {
      verifyToken(token);
    }
  }, []);

  const register = async (username, email, password, firstName = '', lastName = '') => {
    try {
      setLoading(true);
      const response = await api.post('/auth/register', {
        username,
        email,
        password,
        firstName,
        lastName,
      });

      if (response.data.success) {
        return { success: true, message: 'Registration successful' };
      }
      return { success: false, message: response.data.message };
    } catch (error) {
      return { success: false, message: error.response?.data?.error || error.message };
    } finally {
      setLoading(false);
    }
  };

  const login = async (username, password) => {
    try {
      setLoading(true);
      const response = await api.post('/auth/login', { username, password });

      if (response.data.success) {
        const { accessToken, refreshToken, user: userData } = response.data;
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        setToken(accessToken);
        setUser(userData);
        return { success: true, user: userData };
      }
      return { success: false, message: response.data.message };
    } catch (error) {
      return { success: false, message: error.response?.data?.error || error.message };
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    setToken(null);
    setUser(null);
  };

  const verifyToken = async (t) => {
    try {
      const response = await api.post('/auth/verify', { token: t });
      if (response.data.userId) {
        const userResponse = await api.get('/auth/profile', { params: { token: t } });
        setUser(userResponse.data);
        return true;
      }
      return false;
    } catch (error) {
      logout();
      return false;
    }
  };

  const refreshAccessToken = async () => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) return false;

      const response = await api.post('/auth/refresh', { refreshToken });
      if (response.data.success) {
        localStorage.setItem('accessToken', response.data.accessToken);
        localStorage.setItem('refreshToken', response.data.refreshToken);
        setToken(response.data.accessToken);
        setUser(response.data.user);
        return true;
      }
      return false;
    } catch (error) {
      logout();
      return false;
    }
  };

  const value = {
    user,
    token,
    loading,
    register,
    login,
    logout,
    refreshAccessToken,
    isAuthenticated: !!user,
    isAdmin: user?.role === 'Admin',
    isEditor: user?.role === 'Editor',
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
