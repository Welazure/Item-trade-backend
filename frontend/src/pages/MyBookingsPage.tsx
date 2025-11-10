import React, { useState, useEffect } from 'react';
import api from '../api';

interface Booking {
  id: string;
  itemName: string;
  itemOwner: {
    name: string;
    phoneNumber: string;
    email: string;
  };
}

const MyBookingsPage: React.FC = () => {
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchBookings = async () => {
    try {
      setLoading(true);
      const response = await api.get<Booking[]>('/booking/my-bookings');
      setBookings(response.data);
    } catch (err) {
      setError('Failed to fetch your bookings.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBookings();
  }, []);

  const handleCancel = async (bookingId: string) => {
    if (window.confirm('Are you sure you want to cancel this booking?')) {
      try {
        await api.put(`/booking/${bookingId}/cancel`);
        fetchBookings(); // Refetch to update the list
      } catch (err) {
        alert('Failed to cancel booking.');
        console.error(err);
      }
    }
  };

  if (loading) return <p>Loading your bookings...</p>;
  if (error) return <p style={{ color: 'var(--color-error)' }}>{error}</p>;

  return (
    <div>
      <h1>My Bookings</h1>
      <p>Here are the items you've booked. Contact the owner to arrange the trade.</p>
      <table style={{ width: '100%', textAlign: 'left', borderCollapse: 'collapse' }}>
        <thead>
          <tr>
            <th style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>Item Name</th>
            <th style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>Owner</th>
            <th style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>Owner Contact</th>
            <th style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {bookings.map(booking => (
            <tr key={booking.id}>
              <td style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>{booking.itemName}</td>
              <td style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>{booking.itemOwner.name}</td>
              <td style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>
                {booking.itemOwner.email} / {booking.itemOwner.phoneNumber}
              </td>
              <td style={{ padding: '0.75rem', borderBottom: '1px solid var(--color-border)' }}>
                <button onClick={() => handleCancel(booking.id)} className="btn btn-secondary">Cancel Booking</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default MyBookingsPage;
