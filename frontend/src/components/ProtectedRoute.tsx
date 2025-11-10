import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const ProtectedRoute: React.FC = () => {
  const { isAuthenticated, token } = useAuth();

  // Check auth on component mount to handle page reloads
  React.useEffect(() => {
    const check = async () => {
        const { checkAuth } = useAuth();
        await checkAuth();
    }
    check();
  }, [token]);

  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
};

export default ProtectedRoute;
