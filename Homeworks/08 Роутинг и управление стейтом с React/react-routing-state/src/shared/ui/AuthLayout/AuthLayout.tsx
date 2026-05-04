import { ReactNode } from 'react';
import { Container, Paper, Typography, Box } from '@mui/material';

interface AuthLayoutProps {
  title: string;
  footer?: ReactNode;
  children: ReactNode;
  onSubmit?: (e: React.FormEvent) => void;
}

export const AuthLayout = ({ title, footer, children, onSubmit }: AuthLayoutProps) => {
  return (
    <Container component="main" maxWidth="xs">
      <Paper
        elevation={3}
        sx={{ p: 4, mt: 8, display: 'flex', flexDirection: 'column', alignItems: 'center' }}
      >
        <Typography component="h1" variant="h5" gutterBottom>
          {title}
        </Typography>
        <Box component="form" onSubmit={onSubmit} sx={{ mt: 1, width: '100%' }}>
          {children}
        </Box>
        {footer && <Box sx={{ mt: 2, textAlign: 'center', width: '100%' }}>{footer}</Box>}
      </Paper>
    </Container>
  );
};
