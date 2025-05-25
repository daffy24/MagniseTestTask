# Magnise Test Task

## Description

This project is an API service designed to provide price information for specific market assets (e.g., EUR/USD, GOOG, etc.). The service is built with .NET Core and uses Docker for containerization. It integrates with the Fintacharts platform to fetch real-time and historical pricing data. The instructions below guide you through building and running the application using Docker.

---

## Prerequisites

To run this application, ensure you have the following installed:

* [Docker](https://www.docker.com/) (Docker Desktop if on Windows or macOS)

---

## Procedure to Run the Application

### 1. Clone the Repository

Clone the repository to your local machine:

```bash
git clone https://github.com/daffy24/MagniseTestTask
cd MagniseTestTask
```

### 2. Build the Docker Image

Use the following command to build the Docker image:

```bash
docker build -t magnise-test-task .
```

This command creates a Docker image with the tag `magnise-test-task` based on the provided `Dockerfile`.

### 3. Run the Docker Container

Run the container using the following command:

```bash
docker run -d -p 8080:8080 --name magnisetask magnise-test-task:latest
```

### 4. Verify the Application

Open your browser and navigate to:

```
http://localhost:8080
```

You should see the application running.

---

## Stopping the Application

To stop the running container, use:

```bash
docker stop magnisetask
```

To remove the container, use:

```bash
docker rm magnisetask
```

---

