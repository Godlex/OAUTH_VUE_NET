# Build
FROM node:22-alpine AS build
WORKDIR /app

# Build-time URLs for Docker (browser calls host)
ARG VITE_AUTHORITY=http://localhost:5000
ARG VITE_API_URL=http://localhost:5002
ENV VITE_AUTHORITY=$VITE_AUTHORITY
ENV VITE_API_URL=$VITE_API_URL

COPY frontend/package.json frontend/package-lock.json* ./
RUN npm ci

COPY frontend/ .
RUN npm run build

# Serve with nginx; logs to stdout/stderr for Docker
FROM nginx:alpine AS final
# Log to stdout/stderr so `docker logs` works
RUN ln -sf /dev/stdout /var/log/nginx/access.log \
 && ln -sf /dev/stderr /var/log/nginx/error.log

COPY --from=build /app/dist /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
