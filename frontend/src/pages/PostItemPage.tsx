import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api';

interface Category {
  id: string;
  name: string;
}

const PostItemPage: React.FC = () => {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [request, setRequest] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [categories, setCategories] = useState<Category[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const response = await api.get<Category[]>('/item/categories');
        setCategories(response.data);
        if (response.data.length > 0) {
          setCategoryId(response.data[0].id); // Default to the first category
        }
      } catch (err) {
        console.error('Failed to fetch categories', err);
      }
    };
    fetchCategories();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await api.post('/item', { name, description, request, categoryId });
      const newItemId = response.data.id;
      navigate(`/upload-media/${newItemId}`);
    } catch (err) {
      alert('Failed to create item. Please try again.');
      console.error(err);
    }
  };

  return (
    <div className="form-container">
      <h1>Post a New Item</h1>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="name">Item Name</label>
          <input type="text" id="name" className="form-control" value={name} onChange={e => setName(e.target.value)} required />
        </div>
        <div className="form-group">
          <label htmlFor="description">Description</label>
          <textarea id="description" rows={4} className="form-control" value={description} onChange={e => setDescription(e.target.value)} required></textarea>
        </div>
        <div className="form-group">
          <label htmlFor="request">What you want in return</label>
          <textarea id="request" rows={3} className="form-control" value={request} onChange={e => setRequest(e.target.value)} required></textarea>
        </div>
        <div className="form-group">
          <label htmlFor="category">Category</label>
          <select id="category" className="form-control" value={categoryId} onChange={e => setCategoryId(e.target.value)} required>
            {categories.map(cat => (
              <option key={cat.id} value={cat.id}>{cat.name}</option>
            ))}
          </select>
        </div>
        <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>Submit and Add Media</button>
      </form>
    </div>
  );
};

export default PostItemPage;
