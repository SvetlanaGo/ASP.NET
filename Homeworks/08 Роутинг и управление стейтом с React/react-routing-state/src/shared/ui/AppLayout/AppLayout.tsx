import { Suspense } from 'react';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAppSelector, useAppDispatch } from '@/app/store/hooks';
import { logout } from '@/app/store/slices/auth.slice';
import { AppBar, Toolbar, Typography, Button, Box, Container } from '@mui/material';

export const AppLayout = () => {
  const auth = useAppSelector((state) => state.auth);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <Typography
            variant="h6"
            component={Link}
            to="/"
            sx={{
              flexGrow: 1,
              textDecoration: 'none',
              color: 'inherit',
              '&:hover': { opacity: 0.9 },
            }}
          >
            REACT-ПРИЛОЖЕНИЕ
          </Typography>

          <Box sx={{ display: 'flex', gap: 1 }}>
            {auth.isAuthenticated ? (
              <>
                <Button color="inherit" component={Link} to="/">
                  Главная
                </Button>
                <Button color="inherit" onClick={handleLogout}>
                  Выйти
                </Button>
              </>
            ) : (
              <>
                <Button color="inherit" component={Link} to="/login">
                  Вход
                </Button>
                <Button color="inherit" component={Link} to="/register">
                  Регистрация
                </Button>
              </>
            )}
          </Box>
        </Toolbar>
      </AppBar>

      <Container maxWidth="xl" sx={{ py: 2 }}>
        <Suspense fallback={<Box sx={{ textAlign: 'center', py: 4 }}>Загрузка...</Box>}>
          <Outlet />
        </Suspense>
      </Container>
    </>
  );
};
