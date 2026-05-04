import { ComponentType } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAppSelector } from '@/app/store/hooks';

export function withAuth<P extends object>(WrappedComponent: ComponentType<P>) {
  const displayName = WrappedComponent.displayName || WrappedComponent.name || 'Component';

  const ComponentWithAuth = (props: P) => {
    const isAuthenticated = useAppSelector((state) => state.auth.isAuthenticated);
    const location = useLocation();

    if (!isAuthenticated) {
      return <Navigate to="/login" state={{ from: location }} replace />;
    }

    return <WrappedComponent {...props} />;
  };

  ComponentWithAuth.displayName = `withAuth(${displayName})`;
  return ComponentWithAuth;
}
