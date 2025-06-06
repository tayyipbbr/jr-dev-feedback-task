# jr-dev-feedback-task
"Full-stack feedback application using .NET 8, ReactJS, RabbitMQ, and MongoDB."

FeedbackApp Projesi
FeedbackApp, kullanıcıların geri bildirimlerini gönderebildiği, bu geri bildirimlerin bir API aracılığıyla alınıp bir mesaj kuyruğuna iletildiği ve ardından bir worker servis tarafından işlenerek veritabanına kaydedildiği basit bir full-stack uygulamasıdır.

Kullanılan Teknolojiler
Frontend: ReactJS (Vite ile oluşturulmuş), Tailwind CSS

Backend API: ASP.NET Core 8 Web API

Mesaj Kuyruğu (Queue): RabbitMQ (MassTransit ile entegre)

Veritabanı (Database): MongoDB

Containerization (Bağımlı Servisler İçin): Docker, Docker Compose

Proje Mimarisi
Uygulama genel olarak aşağıdaki akışla çalışır:

Kullanıcı (Frontend): React arayüzündeki formu doldurarak geri bildirimini gönderir.

ASP.NET Core Web API: Frontend'den gelen isteği /api/feedback endpoint'inde karşılar, veriyi doğrular ve bir mesaj olarak MassTransit aracılığıyla RabbitMQ'ya yayınlar.w

RabbitMQ: API'den gelen mesajı alır ve ilgili kuyruğa yönlendirir.

ASP.NET Core Worker Service: RabbitMQ'daki kuyruğu dinler, yeni bir mesaj geldiğinde onu alır, işler ve MongoDB veritabanına kaydeder.

MongoDB: Alınan geri bildirimleri kalıcı olarak saklar.

-------------------------------------------------------------

Neleri Geliştirebilirdim?

-proxy/reverse proxy #
-middleware çalışmasının detaylandırılması #
-Detaylı UI #
-Çoklu rabbitmq işlev testi #

-------------------------------------------------------------

Kurulum ve Çalıştırma Adımları
Bu projeyi yerel makinenizde çalıştırmak için aşağıdaki adımları izleyin:

1. Ön Gereksinimler
Makinenizde aşağıdaki araçların kurulu olduğundan emin olun:

.NET 8 SDK veya üzeri

Node.js (LTS sürümü önerilir, npm içerir)

Docker Desktop

2. Projeyi Klonlama
Bu repoyu yerel makinenize klonlayın:

git clone <REPO_URL>
cd <REPO_KLASOR_ADI>

3. Bağımlı Servisleri Docker ile Başlatma (RabbitMQ ve MongoDB)
Projenin kök dizininde bulunan docker-compose.yml dosyası, RabbitMQ ve MongoDB servislerini kolayca başlatmanızı sağlar.

Terminalde projenin kök dizinindeyken aşağıdaki komutu çalıştırın:

docker-compose up -d

Bu komut, servisleri arka planda başlatacaktır.

RabbitMQ Yönetim Arayüzü: http://localhost:15672 (Kullanıcı: guest, Şifre: guest)

MongoDB Bağlantısı: mongodb://localhost:27017

-----------------------------------------------------------

4. Backend Kurulumu ve Çalıştırılması (API ve Worker Service)
API Projesi (FeedbackApp.Api):

API projesinin klasörüne gidin (örneğin, cd FeedBackApp.API).

Bağımlılıkları yükleyin:

dotnet restore

appsettings.json dosyasındaki RabbitMq:Host değerinin Docker Compose'daki RabbitMQ servis adına (varsayılan olarak localhost veya rabbitmq eğer API de Docker içindeyse) ayarlandığından emin olun. Yerel geliştirme için localhost genellikle çalışır.

API'yi çalıştırın:

dotnet run

API genellikle https://localhost:7185 (veya http://localhost:5185) gibi bir adreste başlayacaktır. Bu adres, API projenizin Properties/launchSettings.json dosyasında yapılandırılmıştır.
Swagger arayüzüne genellikle /swagger yolundan erişebilirsiniz (örn: https://localhost:7185/swagger).

Worker Service Projesi (FeedbackApp.WorkerService):

Worker Service projesinin klasörüne gidin (örneğin, cd FeedbackApp.WorkerService).

Bağımlılıkları yükleyin:

dotnet restore

appsettings.json dosyasındaki RabbitMq:Host ve MongoDb:ConnectionString değerlerinin doğru olduğundan emin olun. RabbitMQ için localhost, MongoDB için mongodb://localhost:27017 varsayılan değerlerdir.

Worker Service'i çalıştırın:

dotnet run

------------------------------------------------------------------------------

5. Frontend Kurulumu ve Çalıştırılması (feedback-frontend)
Frontend projesinin klasörüne gidin:

cd feedback-frontend

Bağımlılıkları yükleyin:

npm install

Ortam Değişkenlerini Ayarlama:

Projenin kök dizininde (feedback-frontend klasörünün içinde) .env.example adında bir dosya bulacaksınız. Bu dosya, gerekli ortam değişkenlerini gösterir.

.env.example dosyasını kopyalayıp aynı dizinde .env.development adında yeni bir dosya oluşturun.

.env.development dosyasını açın ve içindeki VITE_API_BASE_URL değişkenini çalışan backend API'nizin adresine göre güncelleyin. API'niz varsayılan olarak https://localhost:7185 adresinde çalışıyorsa, dosyanın içeriği şöyle olmalıdır:

VITE_API_BASE_URL=https://localhost:7185/api

Eğer API projenizin HTTPS portu farklıysa (örneğin 7001), 7185 yerine o portu yazın.

Frontend geliştirme sunucusunu başlatın:

npm run dev

Uygulama genellikle http://localhost:5173 adresinde açılacaktır.

----------------------------------------------------

6. .NET Uygulamaları ve Docker
Bu projenin mevcut yapılandırmasında:

MongoDB ve RabbitMQ: docker-compose.yml dosyası aracılığıyla Docker container'ları olarak çalıştırılır. Bu, bu servislerin kurulumunu ve yönetimini basitleştirir.

ASP.NET Core API ve Worker Service: Doğrudan yerel makinenizde .NET SDK kullanılarak çalıştırılır (Dockerize edilmemiştir).

Eğer .NET uygulamalarını da Docker içinde çalıştırmak isterseniz, her bir .NET projesi (API ve Worker Service) için ayrı Dockerfile dosyaları oluşturmanız ve bunları docker-compose.yml dosyanıza servis olarak eklemeniz gerekir. Bu, genellikle production ortamları için tercih edilen bir yaklaşımdır ve projenin taşınabilirliğini artırır. Bu başlangıç seviyesi proje için, sadece harici bağımlılıkların (MongoDB, RabbitMQ) Docker ile yönetilmesi yeterli görülmüştür.

API Endpoint'leri
POST /api/feedback: Yeni bir geri bildirim alır.

Request Body (JSON):

{
  "name": "string",
  "email": "string (valid email format)",
  "message": "string"
}

Başarılı Yanıt (200 OK):

{
  "message": "Geri bildiriminiz başarıyla alındı ve işlenmek üzere sıraya eklendi."
}

Hatalı Yanıt (400 Bad Request, 500 Internal Server Error vb.): ProblemDetails formatında hata detayı döner.

Testler
Projede backend API ve Worker Service için temel birim testleri bulunmaktadır. Bu testleri çalıştırmak için solution'ın kök dizininde aşağıdaki komutu kullanabilirsiniz:

dotnet test



