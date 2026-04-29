import axios from 'axios';
import { useState } from 'react';
import SuccessBox from './SuccessBox';
import ErrorBox from './ErrorBox';
import './ApiFetcher.css';

interface CatFact {
  fact: string;
  length: number;
}

interface ApiResponse {
  data: CatFact[];
}

const API_URL = 'https://catfact.ninja/facts';

export default function ApiFetcher() {
  const [data, setData] = useState<ApiResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(false);

  const fetchData = async () => {
    setLoading(true);
    setError(null);

    try {
      const { data: response } = await axios.get<ApiResponse>(API_URL);
      if (!response?.data || response.data.length === 0) {
          throw new Error('No facts returned from API. Please try again later.');
      }

      setData(response);
    } catch (err) {
      const message = axios.isAxiosError(err) 
        ? err.response?.data?.message || err.message 
        : err instanceof Error 
          ? err.message 
          : 'Unknown error occurred';
      
      setData(null);
      setError(message);
      
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="api-fetcher">
      <button 
        onClick={fetchData} 
        disabled={loading}
        className={`fetch-button ${loading ? 'loading' : ''}`}
        aria-busy={loading}
        aria-label="Get random facts about cats from API"
      >
        {loading ? 'Loading...' : 'Get facts about cats'}
      </button>

      {error && <ErrorBox message={error} />}
      {data && <SuccessBox data={data} loading={loading} />}
    </div>
  );
}