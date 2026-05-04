import { lazy } from 'react';

export const lazyPages = {
  Login: lazy(() => import('@/pages/auth/LoginPage')),
  Register: lazy(() => import('@/pages/auth/RegisterPage')),
  Home: lazy(() => import('@/pages/home')),
  NotFound: lazy(() => import('@/pages/not-found')),
};
