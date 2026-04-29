import ApiFetcher from './components/ApiFetcher';
import './App.css';

function App() {
  return (
    <div className="app">
      <h1 className="app__title">Cat Facts API</h1>
      <p className="app__description">
        Click the button to get facts about cats from the public API.
      </p>
      
      <ApiFetcher />
    </div>
  );
}

export default App;