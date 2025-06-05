import React from 'react';
import { CheckCircle, XCircle, Loader } from 'lucide-react';

function Notification({ message, type }) {
  if (!message) return null;

  const baseStyle = "p-3.5 mb-4 text-sm rounded-lg shadow-md flex items-center font-medium";
  const typeStyles = {
    success: "bg-green-500 text-white",
    error: "bg-red-600 text-white",
    loading: "bg-accent-blue text-white"
  };
  
  const iconMap = {
     success: <CheckCircle className="h-5 w-5 mr-2.5 shrink-0" />,
     error: <XCircle className="h-5 w-5 mr-2.5 shrink-0" />,
     loading: <Loader className="animate-spin h-5 w-5 mr-2.5 text-white shrink-0" />
  };

  return (
    <div className={`${baseStyle} ${typeStyles[type] || typeStyles.loading}`} role="alert">
      {iconMap[type] || iconMap.loading}
      <span>{message}</span>
    </div>
  );
}

export default Notification;