import React from 'react';
import ReactDOM from 'react-dom/client';
import { ReduxProvider } from '@/app/providers/ReduxProvider';
import { AppRouter } from '@/app/router';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <ReduxProvider>
      <AppRouter />
    </ReduxProvider>
  </React.StrictMode>,
);
