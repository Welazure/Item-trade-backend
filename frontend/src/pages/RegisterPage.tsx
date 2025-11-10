import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../api';

const RegisterPage: React.FC = () => {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    name: '',
    address: '',
    phoneNumber: '',
  });
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.id]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const response = await api.post('/auth/register', formData);
      login(response.data.token);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data || 'Registration failed. Please try again.');
      console.error(err);
    }
  };

  return (
    <div className="form-container">
      <h1>Create an Account</h1>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="username">Username</label>
          <input type="text" id="username" className="form-control" onChange={handleChange} required />
        </div>
        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input type="email" id="email" className="form-control" onChange={handleChange} required />
        </div>
        <div className="form-group">
          <label htmlFor="password">Password</label>
          <input type="password" id="password" className="form-control" onChange={handleChange} required />
        </div>
        <div className="form-group">
          <label htmlFor="name">Full Name</label>
          <input type="text" id="name" className="form-control" onChange={handleChange} required />
        </div>
        <div className="form-group">
          <label htmlFor="address">Address</label>
          <input type="text" id="address" className="form-control" onChange={handleChange} required />
        </div>
        <div className="form-group">
          <label htmlFor="phoneNumber">Phone Number</label>
          <input type="tel" id="phoneNumber" className="form-control" onChange={handleChange} required />
        </div>
        {error && <p style={{ color: 'var(--color-error)' }}>{error}</p>}
        <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>Register</button>
      </form>
    </div>
  );
};

export default RegisterPage;
