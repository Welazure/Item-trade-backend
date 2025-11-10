import { defineStore } from 'pinia';
import api from '../api';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('authToken') || null,
    user: null, // You can store user info here after login
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
  actions: {
    async login(credentials: { username, password }) {
      try {
        const response = await api.post('/auth/login', credentials);
        this.token = response.data.token;
        localStorage.setItem('authToken', this.token);
        // Optionally fetch and store user details
        // const userResponse = await api.get('/profile/me');
        // this.user = userResponse.data;
        return true;
      } catch (error) {
        console.error('Login failed:', error);
        return false;
      }
    },
    async register(userInfo: any) {
      try {
        const response = await api.post('/auth/register', userInfo);
        this.token = response.data.token;
        localStorage.setItem('authToken', this.token);
        return true;
      } catch (error) {
        console.error('Registration failed:', error);
        throw error; // Re-throw to be caught in the component
      }
    },
    logout() {
      this.token = null;
      this.user = null;
      localStorage.removeItem('authToken');
    },
  },
});
