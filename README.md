# Mango.Web

## Running Docker Containers - via any terminal

Building Image:-
docker build -f Frontend/Mango.Web/Dockerfile -t mango-web:local .

Running Container:-
docker run --name mango-web --env-file Frontend/Mango.Web/.env -p 5048:8080 mango-web:local
