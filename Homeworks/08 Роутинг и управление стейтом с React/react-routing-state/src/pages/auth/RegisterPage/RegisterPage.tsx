import { useState, FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAppDispatch } from '@/app/store/hooks';
import { loginSuccess } from '@/app/store/slices/auth.slice';
import { TextField, Button, Typography, Link as MuiLink } from '@mui/material';
import { AuthLayout } from '@/shared/ui/AuthLayout';

const useRegisterForm = () => {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();

    dispatch(
      loginSuccess({
        id: Date.now().toString(),
        email,
        name: name || email.split('@')[0],
      }),
    );

    navigate('/');
  };

  return {
    name,
    email,
    password,
    setName,
    setEmail,
    setPassword,
    handleSubmit,
  };
};

const RegisterFooter = () => (
  <Typography>
    Уже есть аккаунт?{' '}
    <MuiLink component={Link} to="/login" underline="hover">
      Войти
    </MuiLink>
  </Typography>
);

export const RegisterPage = () => {
  const form = useRegisterForm();

  return (
    <AuthLayout title="Регистрация" footer={<RegisterFooter />} onSubmit={form.handleSubmit}>
      <TextField
        margin="normal"
        required
        fullWidth
        label="Имя"
        value={form.name}
        onChange={(e) => form.setName(e.target.value)}
        autoComplete="name"
      />
      <TextField
        margin="normal"
        required
        fullWidth
        label="Email"
        type="email"
        value={form.email}
        onChange={(e) => form.setEmail(e.target.value)}
        autoComplete="email"
      />
      <TextField
        margin="normal"
        required
        fullWidth
        label="Пароль"
        type="password"
        value={form.password}
        onChange={(e) => form.setPassword(e.target.value)}
        autoComplete="new-password"
      />
      <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
        Создать аккаунт
      </Button>
    </AuthLayout>
  );
};

export default RegisterPage;
