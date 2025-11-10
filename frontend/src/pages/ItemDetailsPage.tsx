import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api';

interface Item {
  id: string;
  name: string;
  description: string;
  request: string;
  media: { filePath: string }[];
  user: { name: string };
}

const ItemDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [item, setItem] = useState<Item | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const fetchItem = async () => {
      try {
        setLoading(true);
        const response = await api.get<Item>(`/item/${id}`);
        setItem(response.data);
      } catch (err) {
        setError('Failed to fetch item details.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchItem();
    }
  }, [id]);

  const handleBooking = async () => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      navigate('/login');
      return;
    }

    try {
      await api.post(`/booking/${id}`);
      alert('Item booked successfully! You can view it in "My Bookings".');
      navigate('/');
    } catch (err: any) {
      alert(`Booking failed: ${err.response?.data || 'Please try again.'}`);
      console.error(err);
    }
  };

  if (loading) return <p>Loading details...</p>;
  if (error) return <p style={{ color: 'var(--color-error)' }}>{error}</p>;
  if (!item) return <p>Item not found.</p>;

  return (
    <div>
      <h1>{item.name}</h1>
      <div style={{ display: 'flex', gap: '2rem', flexWrap: 'wrap' }}>
        <img 
          src={item.media.length > 0 ? item.media[0].filePath : `https://picsum.photos/seed/${id}/600/400`} 
          alt={item.name} 
          style={{ maxWidth: '50%', borderRadius: 'var(--border-radius)' }} 
        />
        <div>
          <h3>Description</h3>
          <p>{item.description}</p>
          <h3>Owner's Request</h3>
          <p>{item.request}</p>
          <p><strong>Posted by:</strong> {item.user.name}</p>
          <button onClick={handleBooking} className="btn btn-primary" style={{ marginTop: '1rem' }}>Book this Item</button>
        </div>
      </div>
    </div>
  );
};

export default ItemDetailsPage;
