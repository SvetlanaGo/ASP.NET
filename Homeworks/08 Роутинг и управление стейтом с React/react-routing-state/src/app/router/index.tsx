import { RouterProvider } from 'react-router-dom';
import { router } from './router';
import { Suspense } from 'react';

export const AppRouter = () => (
  <Suspense fallback={<div>Загрузка...</div>}>
    <RouterProvider router={router} />
  </Suspense>
);

export default AppRouter;
