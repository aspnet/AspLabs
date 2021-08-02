import React, { useState } from 'react';
import logo from './logo.svg';
import './App.css';

function App() {
  const [nextCounterIndex, setNextCounterIndex] = useState(1);
  const [blazorCounters, setBlazorCounters] = useState([]);
  const addBlazorCounter = () => {
    const index = nextCounterIndex;
    setNextCounterIndex(index + 1);
    setBlazorCounters(blazorCounters.concat([{
      title: `Counter ${index}`,
      incrementAmount: index,
    }]));
  };
  const removeBlazorCounter = () => {
    setBlazorCounters(blazorCounters.slice(0, -1));
  };

  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          <button onClick={addBlazorCounter}>Add Blazor counter</button> &nbsp;
          <button onClick={removeBlazorCounter}>Remove Blazor counter</button>
        </p>

        {blazorCounters.map(counter =>
          <div key={counter.title}>
            <my-counter title={counter.title} increment-amount={counter.incrementAmount}></my-counter>
          </div>
        )}

        
      </header>
    </div>
  );
}

export default App;
