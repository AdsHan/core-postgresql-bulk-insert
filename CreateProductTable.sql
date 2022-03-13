CREATE DATABASE bulkinsertdb;

CREATE TABLE public.products(
	id SERIAL PRIMARY KEY,
    sku character varying(120) NOT NULL,
    name character varying(250) NOT NULL,
    url character varying(120) NOT NULL
);