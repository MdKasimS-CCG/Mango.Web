# Mango.Web

Mango.Web is the browser-facing ASP.NET Core MVC application for the Mango shopping application. It uses Razor views and communicates with backend Mango APIs through typed/service-layer HTTP calls.

## Contents

- [Overview](#overview)
- [Features](#features)
- [Technology](#technology)
- [Checkout behaviour](#checkout-behaviour)
- [Reward points widget](#reward-points-widget)
- [Architecture and integrations](#architecture-and-integrations)
- [Configuration](#configuration)
- [Prerequisites](#prerequisites)
- [Run locally](#run-locally)
- [Run with Docker](#run-with-docker)
- [Docker Setup Commands - Can Be Used Via Any Terminal](#run-docker-containers---via-any-terminal)
- [Run in the full Mango Compose stack](#run-in-the-full-mango-compose-stack)
- [CI/CD](#cicd)
- [Application pages and authentication](#application-pages-and-authentication)
- [Troubleshooting](#troubleshooting)

## Overview

Mango.Web provides the user interface for browsing products, viewing product details, managing a cart, applying coupons, authenticating, and initiating checkout/order flows. The application is an ASP.NET Core MVC project with Razor `.cshtml` views.

## Features

The repository contains MVC controllers and views for:
- Home and product listing/details
- Product create, edit, and delete pages
- Login and registration
- Cart view, checkout, and confirmation
- Coupon listing and coupon create/edit/delete pages
- A reward-points widget in the page layout for signed-in users (see [Reward points widget](#reward-points-widget))

Some flows are represented by UI/controller scaffolding and depend on backend services. Review the corresponding controller and service implementation before treating a workflow as fully operational. For example, the checkout controller still has a TODO around payment.

## Checkout behaviour

`POST /Cart/Checkout` is protected with an anti-forgery token (`[ValidateAntiForgeryToken]`); the checkout form includes the token. The action:

1. Validates that the posted cart header is present and that name, email and phone are filled in. If not, it redirects back to Checkout with an error toast instead of failing with a null reference.
2. Creates the order through OrderAPI.
3. Clears the cart through ShoppingCartAPI (`ClearCart/{userId}`, which only allows the signed-in user to clear their own cart).
4. If creating the order fails, the user is sent back to the cart with an error toast. If the order was created but the cart could not be cleared (no `sub` claim, a failed response, or an exception), the failure is logged with `ILogger` at error level and the user still reaches the confirmation page with a warning toast. The `warning` TempData value is rendered by `_Notifications.cshtml`.

Mango.Web does **not** reference `Mango.MessageBus` and never talks to RabbitMQ. The emails and reward points that follow an order are produced asynchronously: OrderAPI publishes the order-created events with `Mango.MessageBus` 3.0.0, and EmailAPI and RewardAPI consume them. The "cart email" action calls ShoppingCartAPI's `EmailCartRequest`, which publishes through the same package.

## Reward points widget

`RewardPointsViewComponent` is rendered from the layout on every page for authenticated users, so it is built to fail soft:
- One call to RewardAPI per refresh (`GetRewardsForUserAsync`); the total is computed from the history returned.
- A 2-second timeout, linked to the request's cancellation token (`BaseService` accepts an optional `CancellationToken`).
- The result is cached per user in `IMemoryCache` under `rewards:{userId}` for 5 minutes. Failures are cached for 30 seconds so an outage does not add the timeout to every page.
- If RewardAPI is unavailable or times out, nothing is rendered and a warning is logged; the page still loads.
- The cached total can be up to 5 minutes stale after a checkout.

## Technology

- .NET 10 / ASP.NET Core MVC
- Razor views
- Bootstrap, jQuery and jQuery Validation assets
- HTTP service classes for backend APIs
- JWT token provider and bearer-token handling
- Docker and GitHub Actions

## Architecture and integrations

```text
Browser
   |
   v
Mango.Web (ASP.NET Core MVC / Razor)
   |---- AuthAPI
   |---- ProductAPI
   |---- CouponAPI
   |---- ShoppingCartAPI
   |---- OrderAPI
   |---- RewardAPI
```

The service classes under `Service/` wrap backend HTTP requests. The `BaseService` is the shared request layer; specific service classes provide API-specific operations. API URLs are environment-dependent.

For a Docker Compose deployment, configure backend URLs with Compose service DNS names and each API's internal listening port (typically 8080), not `localhost` or host-published ports.

## Configuration

Copy `.env.example` to `.env` and fill in the values required for your environment. Do not commit secrets, signing keys, tokens, or production connection details.

Configuration includes:
- ASP.NET Core environment and HTTP port
- Backend API base URLs for Auth, Product, Coupon, ShoppingCart, Order and Reward (`Http__ServiceUrls__RewardAPI` / `Docker__ServiceUrls__RewardAPI`)
- Authentication/token settings consumed by the application

Use the exact variable names in the checked-in `.env.example`. For local execution, backend URLs should target host-published ports (for example `http://localhost:<mapped-port>`). In Compose, override them with the relevant service names, such as `http://mango-product:8080`.

## Prerequisites

- .NET 10 SDK
- Backend Mango services reachable at the configured URLs
- Docker Desktop for container execution

Authentication and commerce workflows require the corresponding backend APIs to be running and configured consistently.

## Run locally

1. Clone the repository and enter the project directory.
2. Copy `.env.example` to `.env`; set backend URLs to the host addresses/ports where the APIs are running.
3. Restore and run:

```bash
dotnet restore Mango.Web.csproj
dotnet run --launch-profile http
```

The checked-in HTTP launch profile should be consulted in `Properties/launchSettings.json` for the exact local URL. The Docker launch profile uses port 8080 inside the container and reads the project `.env` file through its container run arguments.

Open the local URL printed by `dotnet run`. The application uses MVC routes and Razor views; it is not a standalone JSON API with a Swagger-first interface.

## Run with Docker

The Dockerfile uses the .NET 10 ASP.NET runtime and SDK images, exposes 8080/8081, and runs `Mango.Web.dll`.

Build from the Web repository directory:

```bash
docker build -t mango-web:local .
```

Run with environment variables from the project `.env` file:

```bash
docker run --rm --name mango-web \
  --env-file .env \
  -p 5048:8080 \
  mango-web:local
```

Then open `http://localhost:5048` (or the host port you mapped). The backend URLs supplied to the container must be reachable from inside the container.

## Run Docker Containers - via any terminal

Building Image:-
docker build -f Frontend/Mango.Web/Dockerfile -t mango-web:local .

Running Container:-
docker run --name mango-web --env-file Frontend/Mango.Web/.env -p 5048:8080 mango-web:local

## Run in the full Mango Compose stack

Use the standalone Compose file at the Mango solution root, not the Visual Studio-generated Compose files.

A Web service definition should use the Web repository directory as its build context:

```yaml
services:
  mango-web:
    build:
      context: ./Frontend/Mango.Web
      dockerfile: Dockerfile
    env_file:
      - ./Frontend/Mango.Web/.env
    environment:
      ASPNETCORE_HTTP_PORTS: "8080"
      # Override API URLs to Compose service names here.
```

Set API base URLs to the actual Compose service keys and internal ports, for example:
- AuthAPI → `http://mango-auth:8080`
- ProductAPI → `http://mango-product:8080`
- CouponAPI → `http://mango-coupon:8080`
- ShoppingCartAPI → `http://mango-shoppingcart:8080`
- OrderAPI → `http://mango-order:8080`
- RewardAPI → `http://mango-reward:8080`

These are examples based on the service names used in the Mango Compose setup; match them to the actual root Compose file.

From the solution root:

```bash
docker compose -f docker-compose.yml config
docker compose -f docker-compose.yml build mango-web
docker compose -f docker-compose.yml up -d mango-web
docker compose -f docker-compose.yml logs -f mango-web
```

Adjust the Compose filename/service key if your root Compose file differs.

## CI/CD

The workflow is `.github/workflows/web.yaml`. It runs on pushes to `feature/*` and `main`, and on pull requests targeting `main`.

The workflow installs .NET 10, caches NuGet packages, restores and builds the project, performs the configured Docker Buildx image build, and uses GitHub Container Registry for image publication on pushes to `main`. Pull-request builds are validation builds. Review the workflow file in the branch being released for the exact image name, tag, and secret configuration.

The repository does not include an automated test project in the supplied archive; the workflow's test stage should be treated according to the current workflow contents rather than as a comprehensive UI test suite.

## Application pages and authentication

The MVC controllers include `HomeController`, `ProductController`, `AuthController`, `CartController`, and `CouponController`. Razor views are under `Views/`.

| Area | Controller / views |
|---|---|
| Home and product details | `HomeController`; `Views/Home/` |
| Product management | `ProductController`; `Views/Product/` |
| Login and registration | `AuthController`; `Views/Auth/` |
| Cart and checkout | `CartController`; `Views/Cart/` |
| Coupon management | `CouponController`; `Views/Coupon/` |

The application uses a token provider and service-layer requests to call protected backend endpoints. Sign in through the application and ensure the AuthAPI URL and token configuration match the backend deployment.

## Troubleshooting

| Symptom | Checks |
|---|---|
| Web page loads but API data is missing | Check API base URLs, API availability, and container-to-container DNS/ports. |
| Backend requests fail only in Docker | Replace `localhost` in backend URLs with the Compose service name or appropriate host address. |
| Login/register fails | Verify AuthAPI is running, its URL is correct, and its authentication settings are consistent with the Web application. |
| Checkout does not complete end-to-end | Check the Web log and the toast message; payment is still a TODO. Order creation and cart clearing go through OrderAPI and ShoppingCartAPI. |
| Checkout POST returns 400 | The anti-forgery token is missing; submit the form from the Checkout page rather than posting directly. |
| Checkout redirects back with "Please enter your name, email and phone number" | The posted cart header is missing a required field. |
| "Order was created, but your cart could not be cleared" warning | The order was created but ShoppingCartAPI rejected or failed the clear (a 403 means the token's user differs from the cart owner). Check the Web and ShoppingCartAPI logs. |
| Reward points are not shown in the header | RewardAPI is unreachable, slower than 2 seconds, or `ServiceUrls__RewardAPI` is wrong. The widget hides itself and logs a warning; after a failure it stays hidden for up to 30 seconds. |
| Reward points are stale after checkout | The total is cached for up to 5 minutes per user. |
| Order confirmation, but no email or reward appears | These are produced asynchronously by EmailAPI and RewardAPI from OrderAPI's messages (via `Mango.MessageBus` 3.0.0). Check their logs and the `.dlq` queues, not the Web app. |
| Static styles/scripts are missing | Confirm the app is serving `wwwroot` assets and inspect browser developer tools for failed asset requests. |