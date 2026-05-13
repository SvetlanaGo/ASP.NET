import { useAppSelector, useAppDispatch } from '@/app/store/hooks';
import { Button, Typography, Box, Container } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { logout } from '@/app/store/slices/auth.slice';
import { withAuth } from '@/shared/hocs';

const HomePageContent = () => {
  const auth = useAppSelector((state) => state.auth);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Box sx={{ textAlign: 'center' }}>
        <Typography variant="h3" gutterBottom>
          Главная страница
        </Typography>
        <Typography variant="h6" color="success.main" gutterBottom>
          Добро пожаловать, {auth.user?.name}!
        </Typography>
        <Button variant="outlined" color="error" onClick={handleLogout} sx={{ mt: 2 }}>
          Выйти
        </Button>
      </Box>
    </Container>
  );
};

export const HomePage = withAuth(HomePageContent);
