import React, { useState } from 'react';
import { User, Mail, MessageSquare, Loader } from 'lucide-react';

const InputIcon = ({ icon: IconComponent, focused, isTextarea = false }) => (
  <IconComponent 
    className={`h-5 w-5 absolute left-3.5 ${isTextarea ? 'top-3' : 'top-1/2 -translate-y-1/2'} transition-colors ${focused ? 'text-brand-yellow' : 'text-brand-gray-extralight'}`} 
    strokeWidth={1.5} 
  />
);

// TODO: switch harici bir yapı?
const validateField = (name, value) => {
  switch (name) {
    case 'name':
      if (!value.trim()) return 'Adınız ve soyadınız boş bırakılamaz.';
      if (value.trim().length < 3) return 'Adınız en az 3 karakter olmalıdır.';
      return '';
    case 'email':
      if (!value.trim()) return 'E-posta adresi boş bırakılamaz.';
      if (!/\S+@\S+\.\S+/.test(value)) return 'Geçerli bir e-posta adresi giriniz.';
      return '';
    case 'message':
      if (!value.trim()) return 'Mesaj alanı boş bırakılamaz.';
      if (value.trim().length < 10) return 'Mesajınız en az 10 karakter olmalıdır.';
      return '';
    default:
      return '';
  }
};

function FeedbackForm({ onSubmitSuccess, onSubmitError }) {
  const [formData, setFormData] = useState({ name: '', email: '', message: '' });
  const [errors, setErrors] = useState({ name: '', email: '', message: '' });
  const [isLoading, setIsLoading] = useState(false);
  const [focusedField, setFocusedField] = useState(null);

  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL; // VITE_ standartı. example'da açıklandı
  const FEEDBACK_ENDPOINT = '/feedback'; // api endpoint
  const API_URL = `${API_BASE_URL}${FEEDBACK_ENDPOINT}`;

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prevState => ({ ...prevState, [name]: value }));
    if (errors[name]) {
      setErrors(prevErrors => ({ ...prevErrors, [name]: '' }));
    }
  };

  const handleBlur = (e) => {
    const { name, value } = e.target;
    const errorMessage = validateField(name, value);
    setErrors(prevErrors => ({ ...prevErrors, [name]: errorMessage }));
  };

  const validateForm = () => {
    let formIsValid = true;
    const newErrors = { name: '', email: '', message: '' };
    newErrors.name = validateField('name', formData.name);
    if (newErrors.name) formIsValid = false;
    newErrors.email = validateField('email', formData.email);
    if (newErrors.email) formIsValid = false;
    newErrors.message = validateField('message', formData.message);
    if (newErrors.message) formIsValid = false;
    setErrors(newErrors);
    return formIsValid;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) {
      onSubmitError({ message: 'Lütfen formdaki hataları düzeltin.', type: 'error' });
      return;
    }
    setIsLoading(true);
    onSubmitSuccess({ message: 'Geri bildiriminiz işleniyor...', type: 'loading' });
    try {
      const response = await fetch(API_URL, { // Güncellenmiş API_URL kullanılır ---fetch değişmesi gerek
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData),
      });
      if (response.ok) {
        const result = await response.json();
        onSubmitSuccess({ message: result.message || 'Geri bildiriminiz başarıyla alındı!', type: 'success' });
        setFormData({ name: '', email: '', message: '' }); 
        setErrors({ name: '', email: '', message: '' }); 
      } else {
        let errorMessage = 'Bir sunucu hatası oluştu.';
        try {
            const errorResult = await response.json();
            if (response.status === 400 && errorResult.errors) {
                const errorMessages = Object.values(errorResult.errors).flat();
                errorMessage = errorMessages.join(' ');
            } else if (errorResult && errorResult.title && response.status === 400) {
                 errorMessage = errorResult.title;
            } else if (errorResult && errorResult.message) {
                errorMessage = errorResult.message;
            } else {
                errorMessage = `Hata: ${response.status} ${response.statusText || ''}`;
            }
        } catch (parseError) {
            console.error("Hata yanıtı parse hatası:", parseError);
            errorMessage = `Beklenmedik yanıt (HTTP ${response.status}).`;
        }
        onSubmitError({ message: errorMessage, type: 'error' });
      }
    } catch (error) {
      console.error('Ağ hatası:', error);
      onSubmitError({ message: 'Ağ sorunu veya sunucuya ulaşılamıyor.', type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-2">
      <div>
        <div className="relative">
          <InputIcon icon={User} focused={focusedField === 'name'} />
          <input
            type="text" name="name" id="name" value={formData.name} 
            onChange={handleChange} onBlur={handleBlur}
            onFocus={() => setFocusedField('name')}
            className={`pl-12 pr-4 py-3 block w-full bg-brand-gray-light border rounded-lg shadow-sm focus:outline-none text-text-primary placeholder-brand-gray-extralight transition-colors duration-150 
                        ${errors.name ? 'border-error-red ring-2 ring-error-red shadow-input-error' : 'border-brand-gray-light focus:ring-2 focus:ring-brand-yellow focus:border-brand-yellow'}`}
            placeholder="Adınız ve Soyadınız"
            aria-invalid={errors.name ? "true" : "false"}
            aria-describedby={errors.name ? "name-error" : undefined}
          />
        </div>
        <div className="h-5 mt-1">
          {errors.name && <p id="name-error" className="text-xs text-error-red px-1">{errors.name}</p>}
        </div>
      </div>

      <div>
        <div className="relative">
          <InputIcon icon={Mail} focused={focusedField === 'email'} />
          <input
            type="email" name="email" id="email" value={formData.email} 
            onChange={handleChange} onBlur={handleBlur}
            onFocus={() => setFocusedField('email')}
            className={`pl-12 pr-4 py-3 block w-full bg-brand-gray-light border rounded-lg shadow-sm focus:outline-none text-text-primary placeholder-brand-gray-extralight transition-colors duration-150
                        ${errors.email ? 'border-error-red ring-2 ring-error-red shadow-input-error' : 'border-brand-gray-light focus:ring-2 focus:ring-brand-yellow focus:border-brand-yellow'}`}
            placeholder="E-posta Adresiniz"
            aria-invalid={errors.email ? "true" : "false"}
            aria-describedby={errors.email ? "email-error" : undefined}
          />
        </div>
        <div className="h-5 mt-1">
          {errors.email && <p id="email-error" className="mt-1.5 text-xs text-error-red px-1">{errors.email}</p>}
        </div>
      </div>

      <div>
        <div className="relative">
          <InputIcon icon={MessageSquare} focused={focusedField === 'message'} isTextarea={true} />
          <textarea
            name="message" id="message" rows="5" value={formData.message} 
            onChange={handleChange} onBlur={handleBlur}
            onFocus={() => setFocusedField('message')}
            className={`pl-12 pr-4 py-3 block w-full bg-brand-gray-light border rounded-lg shadow-sm focus:outline-none text-text-primary placeholder-brand-gray-extralight transition-colors duration-150
                        ${errors.message ? 'border-error-red ring-2 ring-error-red shadow-input-error' : 'border-brand-gray-light focus:ring-2 focus:ring-brand-yellow focus:border-brand-yellow'}`}
            placeholder="Geri bildiriminizi buraya yazın..."
            aria-invalid={errors.message ? "true" : "false"}
            aria-describedby={errors.message ? "message-error" : undefined}
          ></textarea>
        </div>
         <div className="h-5 mt-1">
          {errors.message && <p id="message-error" className="mt-1.5 text-xs text-error-red px-1">{errors.message}</p>}
        </div>
      </div>

      <div className="pt-2">
        <button
          type="submit"
          disabled={isLoading}
          className="w-full flex items-center justify-center py-3.5 px-4 border border-transparent rounded-lg shadow-lg text-sm font-semibold text-panel-bg bg-brand-yellow hover:bg-brand-yellow-dark focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-panel-bg focus:ring-brand-yellow-dark disabled:opacity-60 disabled:cursor-not-allowed transition-all duration-150 ease-in-out group"
        >
          {isLoading ? (
            <>
              <Loader className="animate-spin h-5 w-5 text-panel-bg mr-3" />
              Gönderiliyor...
            </>
          ) : (
            'Geri Bildirimi Gönder'
          )}
        </button>
      </div>
    </form>
  );
}

export default FeedbackForm;