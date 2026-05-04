import { useState, FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAppDispatch } from '@/app/store/hooks';
import { loginSuccess } from '@/app/store/slices/auth.slice';
import { TextField, Button, Typography, Link as MuiLink, Alert } from '@mui/material';
import { AuthLayout } from '@/shared/ui/AuthLayout';

const useLoginForm = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!email || !password) {
      setError('Заполните все поля');
      return;
    }

    // Mock-логин
    dispatch(
      loginSuccess({
        id: '1',
        email,
        name: email.split('@')[0] || 'User',
      }),
    );

    navigate('/');
  };

  return {
    email,
    password,
    error,
    setEmail,
    setPassword,
    setError,
    handleSubmit,
  };
};

const LoginFooter = () => (
  <Typography>
    Нет аккаунта?{' '}
    <MuiLink component={Link} to="/register" underline="hover">
      Зарегистрироваться
    </MuiLink>
  </Typography>
);

export const LoginPage = () => {
  const form = useLoginForm();

  return (
    <AuthLayout title="Вход в систему" footer={<LoginFooter />} onSubmit={form.handleSubmit}>
      {form.error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => form.setError(null)}>
          {form.error}
        </Alert>
      )}
      <TextField
        margin="normal"
        required
        fullWidth
        label="Email"
        type="email"
        value={form.email}
        onChange={(e) => form.setEmail(e.target.value)}
        autoComplete="email"
        error={!!form.error && !form.email}
      />
      <TextField
        margin="normal"
        required
        fullWidth
        label="Пароль"
        type="password"
        value={form.password}
        onChange={(e) => form.setPassword(e.target.value)}
        autoComplete="current-password"
        error={!!form.error && !form.password}
      />
      <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
        Войти
      </Button>
    </AuthLayout>
  );
};

export default LoginPage;
