import React, { createContext, useCallback, useEffect, useState } from 'react';
import axios from 'axios';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import Header from './components/Header';
import SearchForm from './components/SearchForm';
import Offers from './components/Offers';
import ReservedOffers from './components/ReservedOffers';
import Login from './components/Login';
import Register from './components/Register';
import Stats from './components/Stats';
import './App.css';
import TripList from './components/TripList';

export const AppContext = createContext();

const App = () => {
  const [offers, setOffers] = useState([]);
  const [clientId, setClientId] = useState(null);

  const reactAppHost = process.env.REACT_APP_HOST || 'localhost';
  const reactAppPort = process.env.REACT_APP_PORT || 3000;

  const fetchData = useCallback(async () => {
    try {
      const result = await axios.get(`http://${reactAppHost}:${reactAppPort}/api/Trip/GetAllTrips`);
      const offersWithStatus = result.data.map(offer => ({ ...offer, isReserved: false, isChanged: false }));
      setOffers(offersWithStatus);
    } catch (error) {
      console.error('Error fetching data:', error.response || error.message);
    }
  }, [reactAppHost, reactAppPort]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleSearch = async (searchParams) => {
    try {
      const result = await axios.get(`http://${reactAppHost}:${reactAppPort}/api/Trip/GetTripsByPreferences`, {
        params: searchParams,
      });
      const offersWithStatus = result.data.map(offer => ({ ...offer, isReserved: false, isChanged: false }));
      setOffers(offersWithStatus);
    } catch (error) {
      console.error('Error searching offers:', error.response || error.message);
    }
  };

  const updateOfferStatus = (offerId) => {
    setOffers(prevOffers =>
      prevOffers.map(offer =>
        offer.id === offerId ? { ...offer, isReserved: true } : offer
      )
    );
  };

  const updateOfferInformation = (offerId, changedType, changedValue) => {
    setOffers(prevOffers =>
      prevOffers.map(offer =>
        offer.id === offerId ? { 
          ...offer, 
          [changedType === "Price" ? "price" : changedType.toLowerCase()]: changedValue, 
          isChanged: true,
          changedType,
          changedValue
        } : offer
      )
    );
  };

  const parseOfferId = (message) => {
    const normalizedMessage = message.replace(/\s/g, '').replace('wasjustreserved', '');
    const offerIdPattern = /^[a-f0-9-]{36}$/;
    return offerIdPattern.test(normalizedMessage) ? normalizedMessage : null;
  };

  // SignalR connection setup
  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`http://${reactAppHost}:${reactAppPort}/notificationHub`)
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // connection.on("ReceiveMessage", (message) => {
    //   console.log("Received message:", message);
    // });

    connection.on("ChangedOffer", (message) => {
      console.log("Offer changed:", message);
      const parts = message.split(';');
      if (parts.length === 4) {
        const [rawOfferId, changedType, changedValue, previousValue] = parts;
        const offerId = parseOfferId(rawOfferId);
        if (offerId) {
          console.log("ID of changed offer:", offerId);
          console.log("changedType", changedType);
          console.log("changedValue", changedValue);
          updateOfferInformation(offerId, changedType, changedValue);
        }
      } else {
        console.error('Unexpected message format:', message);
      }
    });

    connection.on("ReceiveMessage", (message) => {
      console.log("ReceiveMessage:", message);
      const offerId = parseOfferId(message);
      console.log("ID:", offerId);
      if (offerId) {
        console.log("PlainID:", offerId);
        updateOfferStatus(offerId);
      }
    });

    connection.start()
      .then(() => console.log("SignalR Connected"))
      .catch((err) => console.error("SignalR Connection Error:", err));

    return () => {
      connection.stop().then(() => console.log("SignalR Disconnected"));
    };
  }, [reactAppHost, reactAppPort]);

  return (
    <AppContext.Provider value={{ clientId, setClientId }}>
      <Router>
        <div className="App">
          <Header onTitleClick={fetchData} />
          <div style={{ flex: 1 }}>
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route path="/user-offers" element={<ReservedOffers />} />
              <Route path="/stats" element={<Stats />} />
              <Route path="/" element={<><SearchForm onSearch={handleSearch} /><Offers offers={offers} /><TripList /></>} />
            </Routes>
          </div>
          <footer style={{ textAlign: 'left', padding: '2px', fontSize: '8px' }}>
            SK, TM, OP, MK, Grafiki: pix4free.org
          </footer>
        </div>
      </Router>
    </AppContext.Provider>
  );
}

export default App;
