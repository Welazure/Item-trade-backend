import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';

// Import your page components
import HomePage from '../pages/HomePage.vue';
import LoginPage from '../pages/LoginPage.vue';
import RegisterPage from '../pages/RegisterPage.vue';
// Import other pages as you create them...
// e.g., import ProfilePage from '../pages/ProfilePage.vue';

const routes = [
  { path: '/', name: 'Home', component: HomePage },
  { path: '/login', name: 'Login', component: LoginPage },
  { path: '/register', name: 'Register', component: RegisterPage },
  // Example of a protected route:
  // { 
  //   path: '/profile', 
  //   name: 'Profile', 
  //   component: ProfilePage,
  //   meta: { requiresAuth: true } 
  // },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// Navigation Guard (Middleware)
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);

  if (requiresAuth && !authStore.isAuthenticated) {
    // Redirect to login if not authenticated
    next({ name: 'Login' });
  } else {
    // Otherwise, proceed
    next();
  }
});

export default router;
