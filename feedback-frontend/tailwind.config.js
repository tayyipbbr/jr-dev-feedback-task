/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'brand-yellow': '#FFD700',
        'brand-yellow-dark': '#E6C200',
        'brand-gray': {
          DEFAULT: '#34495E',
          light: '#5D6D7E',
          extralight: '#AEB6BF',
        },
        'panel-bg': '#2C3E50',
        'panel-bg-light': '#34495E',
      },
      fontFamily: {
       sans: ['Inter', 'sans-serif'],
      }
    },
  },
  plugins: [],
}