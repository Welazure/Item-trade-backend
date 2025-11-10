<template>
  <div class="form-container">
    <h1>Create an Account</h1>
    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label for="username">Username</label>
        <input type="text" id="username" class="form-control" v-model="formData.username" required />
      </div>
      <div class="form-group">
        <label for="email">Email</label>
        <input type="email" id="email" class="form-control" v-model="formData.email" required />
      </div>
      <div class="form-group">
        <label for="password">Password</label>
        <input type="password" id="password" class="form-control" v-model="formData.password" required />
      </div>
      <div class="form-group">
        <label for="name">Full Name</label>
        <input type="text" id="name" class="form-control" v-model="formData.name" required />
      </div>
      <div class="form-group">
        <label for="address">Address</label>
        <input type="text" id="address" class="form-control" v-model="formData.address" required />
      </div>
      <div class="form-group">
        <label for="phoneNumber">Phone Number</label>
        <input type="tel" id="phoneNumber" class="form-control" v-model="formData.phoneNumber" required />
      </div>
      <p v-if="error" class="error-message">{{ error }}</p>
      <button type="submit" class="btn btn-primary" style="width: 100%">Register</button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const formData = ref({
  username: '',
  email: '',
  password: '',
  name: '',
  address: '',
  phoneNumber: '',
});
const error = ref('');
const router = useRouter();
const authStore = useAuthStore();

const handleSubmit = async () => {
  error.value = '';
  try {
    await authStore.register(formData.value);
    router.push('/');
  } catch (err: any) {
    error.value = err.response?.data || 'Registration failed. Please try again.';
  }
};
</script>

<style scoped>
.form-container {
  max-width: 500px;
  margin: 2rem auto;
  padding: 2rem;
  background-color: #34495e;
  border-radius: 8px;
}
.error-message {
  color: #e74c3c;
  margin-bottom: 1rem;
}
</style>
