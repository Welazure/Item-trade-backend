import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../api';

// Define the types based on your backend models
interface Item {
  id: string;
  name: string;
  description: string;
  request: string;
  media: { filePath: string }[];
}

interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

const HomePage: React.FC = () => {
  const [items, setItems] = useState<Item[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchItems = async () => {
      try {
        setLoading(true);
        const response = await api.get<PaginatedResponse<Item>>('/item');
        setItems(response.data.data);
      } catch (err) {
        setError('Failed to fetch items.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchItems();
  }, []);

  if (loading) return <p>Loading items...</p>;
  if (error) return <p style={{ color: 'var(--color-error)' }}>{error}</p>;

  return (
    <div>
      <h1>Available Items</h1>
      <div className="card-grid">
        {items.map(item => (
          <div key={item.id} className="card">
            <img 
              src={item.media.length > 0 ? item.media[0].filePath : `https://picsum.photos/seed/${item.id}/400/200`} 
              alt={item.name} 
              className="card-img" 
            />
            <div className="card-body">
              <h3 className="card-title">{item.name}</h3>
              <p className="card-text">{item.description}</p>
              <Link to={`/item/${item.id}`} className="btn btn-secondary">View Details</Link>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default HomePage;
