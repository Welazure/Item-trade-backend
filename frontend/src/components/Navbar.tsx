import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Navbar: React.FC = () => {
  const { isAuthenticated, logout, user } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  // A simple check for admin role, assuming user object has a 'role' property
  const isAdmin = user?.role === 'Admin';

  return (
    <nav className="navbar">
      <Link to="/" className="navbar-brand">
        Trade<span>Platform</span>
      </Link>
      <div className="nav-links">
        <Link to="/">Home</Link>
        {isAuthenticated ? (
          <>
            <Link to="/profile">Profile</Link>
            {/* <Link to="/my-items">My Items</Link> */}
            {/* <Link to="/my-bookings">My Bookings</Link> */}
            {/* <Link to="/post-item">Post Item</Link> */}
            {isAdmin && <Link to="/admin">Admin</Link>}
            <button onClick={handleLogout} className="btn btn-secondary" style={{ marginLeft: '1rem' }}>Logout</button>
          </>
        ) : (
          <>
            <Link to="/login">Login</Link>
            <Link to="/register">Register</Link>
          </>
        )}
      </div>
    </nav>
  );
};

export default Navbar;
