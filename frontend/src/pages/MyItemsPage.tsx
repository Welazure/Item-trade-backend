import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../api';

interface MyItem {
  id: string;
  name: string;
  isApproved: boolean;
  bookings: any[]; // Simplified for now
}

const MyItemsPage: React.FC = () => {
  const [items, setItems] = useState<MyItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchItems = async () => {
    try {
      setLoading(true);
      const response = await api.get<MyItem[]>('/item/my-items');
      setItems(response.data);
    } catch (err) {
      setError('Failed to fetch your items.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchItems();
  }, []);

  const handleDelete = async (itemId: string) => {
    if (window.confirm('Are you sure you want to delete this item?')) {
      try {
        await api.delete(`/item/${itemId}`);
        // Refetch items after deletion
        fetchItems();
      } catch (err) {
        alert('Failed to delete item.');
        console.error(err);
      }
    }
  };

  const getItemStatus = (item: MyItem): string => {
    if (item.bookings.some(b => b.isActive)) {
      return 'Booked';
    }
    if (item.isApproved) {
      return 'Live';
    }
    return 'Pending Approval';
  };

  if (loading) return <p>Loading your items...</p>;
  if (error) return <p style={{ color: 'var(--color-error)' }}>{error}</p>;

  return (
    <div>
      <h1>My Items</h1>
      <table style={{ width: '100%', textAlign: 'left', borderCollapse: 'collapse' }}>
        <thead>
          <tr>
            <th style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>Item Name</th>
            <th style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>Status</th>
            <th style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {items.map(item => (
            <tr key={item.id}>
              <td style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>{item.name}</td>
              <td style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>{getItemStatus(item)}</td>
              <td style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>
                <Link to={`/item/${item.id}`} className="btn-secondary btn" style={{ marginRight: '1rem' }}>View</Link>
                <button onClick={() => handleDelete(item.id)} className="btn btn-primary" style={{ backgroundColor: 'var(--color-error)' }}>Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default MyItemsPage;
