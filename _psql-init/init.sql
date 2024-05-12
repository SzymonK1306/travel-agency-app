/* TODO: change db name before deploy (also in appsettings) */
DROP DATABASE IF EXISTS hotel;
CREATE DATABASE hotel;
GRANT ALL PRIVILEGES ON DATABASE hotel TO postgres;