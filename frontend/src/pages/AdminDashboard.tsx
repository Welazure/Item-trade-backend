import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../api';

interface PendingItem {
  id: string;
  name: string;
  user: {
    name: string;
  };
}

const AdminDashboard: React.FC = () => {
  const [items, setItems] = useState<PendingItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchPendingItems = async () => {
    try {
      setLoading(true);
      const response = await api.get<PendingItem[]>('/item/pending');
      setItems(response.data);
    } catch (err) {
      setError('Failed to fetch pending items.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPendingItems();
  }, []);

  const handleApprove = async (itemId: string) => {
    try {
      await api.post(`/item/${itemId}/approve`);
      fetchPendingItems(); // Refresh the list
    } catch (err) {
      alert('Failed to approve item.');
      console.error(err);
    }
  };

  const handleReject = async (itemId: string) => {
    if (window.confirm('Are you sure you want to reject and delete this item?')) {
      try {
        await api.delete(`/item/${itemId}`);
        fetchPendingItems(); // Refresh the list
      } catch (err) {
        alert('Failed to reject item.');
        console.error(err);
      }
    }
  };

  if (loading) return <p>Loading pending items...</p>;
  if (error) return <p style={{ color: 'var(--color-error)' }}>{error}</p>;

  return (
    <div>
      <h1>Admin Dashboard: Pending Approvals</h1>
      <table style={{ width: '100%', textAlign: 'left', borderCollapse: 'collapse' }}>
        <thead>
          <tr>
            <th style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>Item Name</th>
            <th style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>Posted By</th>
            <th style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {items.map(item => (
            <tr key={item.id}>
              <td style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>{item.name}</td>
              <td style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>{item.user.name}</td>
              <td style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>
                <Link to={`/item/${item.id}`} className="btn-secondary btn" style={{ marginRight: '1rem' }}>Review</Link>
                <button onClick={() => handleApprove(item.id)} className="btn btn-primary" style={{ marginRight: '1rem' }}>Approve</button>
                <button onClick={() => handleReject(item.id)} className="btn btn-primary" style={{ backgroundColor: 'var(--color-error)' }}>Reject</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default AdminDashboard;
