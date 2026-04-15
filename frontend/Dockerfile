# Stage 1: Build
FROM node:20-alpine AS builder
WORKDIR /app

# Install dependencies and build the application
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

# Stage 2: Export Production files
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=builder /app/.next ./

USER nginx
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]