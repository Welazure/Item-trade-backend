import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api';

const UploadMediaPage: React.FC = () => {
  const { itemId } = useParams<{ itemId: string }>();
  const [file, setFile] = useState<File | null>(null);
  const [isPrimary, setIsPrimary] = useState(true); // Default first image to primary
  const navigate = useNavigate();

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setFile(e.target.files[0]);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!file) {
      alert('Please select a file to upload.');
      return;
    }

    const formData = new FormData();
    formData.append('file', file);
    formData.append('isPrimary', String(isPrimary));

    try {
      await api.post(`/media/upload/${itemId}`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      alert('Media uploaded successfully!');
      // Optionally, allow more uploads or redirect
      navigate(`/item/${itemId}`);
    } catch (err) {
      alert('Upload failed. Please try again.');
      console.error(err);
    }
  };

  return (
    <div className="form-container">
      <h1>Upload Media for Your Item</h1>
      <p>Item ID: {itemId}</p>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="mediaFile">Select Image or Video</label>
          <input type="file" id="mediaFile" className="form-control" onChange={handleFileChange} required />
        </div>
        <div className="form-group" style={{ display: 'flex', alignItems: 'center' }}>
          <input type="checkbox" id="isPrimary" checked={isPrimary} onChange={e => setIsPrimary(e.target.checked)} style={{ marginRight: '10px' }}/>
          <label htmlFor="isPrimary">Set as primary media</label>
        </div>
        <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>Upload</button>
      </form>
    </div>
  );
};

export default UploadMediaPage;
