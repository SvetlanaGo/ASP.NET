import './SuccessBox.css';

interface CatFact {
    fact: string;
    length: number;
  }
  
  interface ApiResponse {
    data: CatFact[];
  }
  
  interface Props {
    data: ApiResponse;
    loading?: boolean;
  }
  
  export default function SuccessBox({ data, loading = false }: Props) {
    const facts = data?.data || [];

    return (
      <div className="success-box" role="status" aria-live="polite">
        {loading && (
          <div className="success-box__overlay" aria-label="Загрузка новых фактов">
            <span className="spinner" /> Updating the facts...
          </div>
        )}
        <h3 className="success-box__title">
          Facts received: {facts.length}
        </h3>
           
        {facts.length > 0 ? (
          <ul className="success-box__list">
            {facts.map((item, index) => (
              <li key={index} className="success-box__item">
                {item.fact}
              </li>
            ))}
          </ul>
        ) : (
          <p className="success-box__empty">No facts to display.</p>
        )}
      </div>
    );
  }