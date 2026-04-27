import './ErrorBox.css';

interface Props {
  message: string;
}

export default function ErrorBox({ message }: Props) {
  return (
    <div className="error-box" role="alert" aria-live="assertive">
      <h3 className="error-box__title">Error!</h3>
      <p className="error-box__message">{message}</p>
    </div>
  );
}