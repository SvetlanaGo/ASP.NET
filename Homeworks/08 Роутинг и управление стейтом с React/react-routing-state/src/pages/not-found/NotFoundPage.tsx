import { Link } from 'react-router-dom';
import { Typography, Container, Button } from '@mui/material';

export const NotFoundPage = () => {
  return (
    <Container maxWidth="sm" sx={{ py: 8, textAlign: 'center' }}>
      <Typography variant="h1" color="error" gutterBottom>
        404
      </Typography>
      <Typography variant="h5" gutterBottom>
        Страница не найдена
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 4 }}>
        Извините, запрашиваемая страница не существует.
      </Typography>
      <Button component={Link} to="/" variant="contained">
        На главную
      </Button>
    </Container>
  );
};
