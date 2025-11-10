<template>
  <div class="form-container">
    <h1>Login</h1>
    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label for="username">Username</label>
        <input type="text" id="username" class="form-control" v-model="username" required />
      </div>
      <div class="form-group">
        <label for="password">Password</label>
        <input type="password" id="password" class="form-control" v-model="password" required />
      </div>
      <p v-if="error" class="error-message">{{ error }}</p>
      <button type="submit" class="btn btn-primary" style="width: 100%">Login</button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const username = ref('');
const password = ref('');
const error = ref('');
const router = useRouter();
const authStore = useAuthStore();

const handleSubmit = async () => {
  error.value = '';
  const success = await authStore.login({
    username: username.value,
    password: password.value,
  });

  if (success) {
    router.push('/');
  } else {
    error.value = 'Login failed. Please check your credentials.';
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
/* Add other relevant styles from your CSS design */
</style>
