import React, { useState, useEffect } from 'react';
import api from '../api';

interface Profile {
  username: string;
  email: string;
  name: string;
  address: string;
  phoneNumber: string;
}

const ProfilePage: React.FC = () => {
  const [profile, setProfile] = useState<Profile | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        setLoading(true);
        const response = await api.get<Profile>('/profile/me');
        setProfile(response.data);
      } catch (err) {
        setError('Failed to fetch profile.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchProfile();
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (profile) {
      setProfile({ ...profile, [e.target.id]: e.target.value });
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!profile) return;
    try {
      await api.put('/profile/me', profile);
      alert('Profile updated successfully!');
    } catch (err: any) {
      alert(`Update failed: ${err.response?.data || 'Please try again.'}`);
      console.error(err);
    }
  };

  if (loading) return <p>Loading profile...</p>;
  if (error) return <p style={{ color: 'var(--color-error)' }}>{error}</p>;
  if (!profile) return <p>Could not load profile.</p>;

  return (
    <div>
      <h1>My Profile</h1>
      <div className="form-container" style={{ maxWidth: 'none' }}>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Username</label>
            <input type="text" className="form-control" value={profile.username} disabled />
          </div>
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input type="email" id="email" className="form-control" value={profile.email} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label htmlFor="name">Full Name</label>
            <input type="text" id="name" className="form-control" value={profile.name} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label htmlFor="address">Address</label>
            <input type="text" id="address" className="form-control" value={profile.address} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label htmlFor="phoneNumber">Phone Number</label>
            <input type="tel" id="phoneNumber" className="form-control" value={profile.phoneNumber} onChange={handleChange} required />
          </div>
          <button type="submit" className="btn btn-primary">Update Profile</button>
        </form>
      </div>
    </div>
  );
};

export default ProfilePage;
