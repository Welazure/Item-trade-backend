import React, { createContext, useState, useContext, ReactNode, useEffect } from 'react';
import api from '../api';

interface AuthContextType {
  token: string | null;
  user: any; // Replace 'any' with a proper User type later
  isAuthenticated: boolean;
  login: (token: string) => void;
  logout: () => void;
  checkAuth: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [token, setToken] = useState<string | null>(localStorage.getItem('authToken'));
  const [user, setUser] = useState<any>(null);

  const checkAuth = async () => {
    const storedToken = localStorage.getItem('authToken');
    if (storedToken && !token) {
        setToken(storedToken);
    }
  };

  useEffect(() => {
    checkAuth();
    if (token) {
      // Optionally fetch user profile if token exists
      api.get('/profile/me')
        .then(response => setUser(response.data))
        .catch(() => {
          // Token might be invalid, so log out
          logout();
        });
    }
  }, [token]);

  const login = (newToken: string) => {
    localStorage.setItem('authToken', newToken);
    setToken(newToken);
  };

  const logout = () => {
    localStorage.removeItem('authToken');
    setToken(null);
    setUser(null);
  };

  const isAuthenticated = !!token;

  return (
    <AuthContext.Provider value={{ token, user, isAuthenticated, login, logout, checkAuth }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
