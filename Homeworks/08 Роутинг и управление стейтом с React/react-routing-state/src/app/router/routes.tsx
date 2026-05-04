import { type ReactNode } from 'react';
import { Navigate, type RouteObject } from 'react-router-dom';
import { AppLayout } from '@/shared/ui/AppLayout';
import { lazyPages } from './lazy-pages';

export const routes: RouteObject[] = [
  {
    path: '/',
    element: <AppLayout />,
    children: [
      {
        index: true,
        element: (<lazyPages.Home />) as ReactNode,
      },
      {
        path: 'login',
        element: (<lazyPages.Login />) as ReactNode,
      },
      {
        path: 'register',
        element: (<lazyPages.Register />) as ReactNode,
      },
      {
        path: '*',
        element: (<lazyPages.NotFound />) as ReactNode,
      },
    ],
  },
  {
    path: '/dashboard',
    element: (<Navigate to="/" replace />) as ReactNode,
  },
];
