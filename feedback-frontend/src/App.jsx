import React, { useState, useEffect } from 'react';
import Notification from './components/Notification';
import FeedbackForm from './components/FeedbackForm';
import { Eye } from 'lucide-react'; 

function App() {
  const [notification, setNotification] = useState({ message: '', type: '' });

  const handleFormNotification = (notificationData) => {
    setNotification(notificationData);
  };

  useEffect(() => {
    if (notification.message && notification.type !== 'loading') {
      const timer = setTimeout(() => {
        setNotification({ message: '', type: '' });
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [notification]);

  return (
    <div className="min-h-screen bg-brand-gray flex flex-col items-center justify-center p-4 font-sans">
      <div className="w-full max-w-lg bg-panel-bg shadow-panel rounded-xl p-8 sm:p-10 space-y-8">

        <div className="text-center">
          <div className="inline-block p-3.5 bg-brand-yellow rounded-full mb-6 shadow-lg">
            <Eye className="h-10 w-10 text-panel-bg" strokeWidth={1.5} />
          </div>
          <h1 className="text-3xl font-bold text-brand-yellow mb-2">Geri Bildirim Paneli</h1>
          <p className="text-brand-gray-extralight text-base">Görüşlerinizle hizmetlerimizi şekillendirin.</p>
        </div>

        <div className="h-16"> 
          {notification.message && <Notification message={notification.message} type={notification.type} />}
        </div>

        <FeedbackForm 
          onSubmitSuccess={handleFormNotification} 
          onSubmitError={handleFormNotification} 
        />

        <p className="text-xs text-brand-gray-extralight text-center mt-10">
          © {new Date().getFullYear()} FeedbackApp360. Tüm hakları Tayyip Biber'e aittir.
        </p>
      </div>
    </div>
  );
}

export default App;