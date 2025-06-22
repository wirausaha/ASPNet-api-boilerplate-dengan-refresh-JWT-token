--
-- PostgreSQL database dump
--

-- Dumped from database version 16.9 (Debian 16.9-1.pgdg120+1)
-- Dumped by pg_dump version 16.9

-- Started on 2025-06-17 07:16:36 UTC

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 216 (class 1259 OID 16389)
-- Name: tbsystoken; Type: TABLE; Schema: public; Owner: postgre
--

-- DROP TABLE IF EXISTS public.tbsystoken;

CREATE TABLE IF NOT EXISTS public.tbsystoken
(
    "Id" integer NOT NULL DEFAULT nextval('"tbsystoken_Id_seq"'::regclass),
    "AccessToken" character varying(2048) COLLATE pg_catalog."default",
    "RefreshToken" character varying(1024) COLLATE pg_catalog."default",
    "CompanyCode" character varying(32) COLLATE pg_catalog."default",
    "UserId" character varying(32) COLLATE pg_catalog."default",
    "UserRole" character varying(32) COLLATE pg_catalog."default",
    "Email" character varying(320) COLLATE pg_catalog."default",
    "ExpireDate" timestamp without time zone,
    "IsExpired" smallint DEFAULT 0,
    CONSTRAINT tbsystoken_pkey PRIMARY KEY ("Id")
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.tbsystoken
    OWNER to postgre;


ALTER TABLE public.tbsystoken OWNER TO postgre;

--
-- TOC entry 215 (class 1259 OID 16388)
-- Name: tbsystoken_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgre
--

CREATE SEQUENCE public."tbsystoken_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."tbsystoken_Id_seq" OWNER TO postgre;

--
-- TOC entry 3380 (class 0 OID 0)
-- Dependencies: 215
-- Name: tbsystoken_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgre
--

ALTER SEQUENCE public."tbsystoken_Id_seq" OWNED BY public.tbsystoken."Id";


--
-- TOC entry 217 (class 1259 OID 16398)
-- Name: users; Type: TABLE; Schema: public; Owner: postgre
--

-- Table: public.users

-- DROP TABLE IF EXISTS public.users;

CREATE TABLE IF NOT EXISTS public.users
(
    "UserId" character varying(32) COLLATE pg_catalog."default" NOT NULL,
    "UserName" character varying(32) COLLATE pg_catalog."default" NOT NULL,
    "Email" character varying(100) COLLATE pg_catalog."default" NOT NULL,
    "Password" character varying(64) COLLATE pg_catalog."default" NOT NULL,
    "FirstName" character varying(32) COLLATE pg_catalog."default",
    "LastName" character varying(32) COLLATE pg_catalog."default",
    "DateOfBirth" date,
    "PhoneNumber" character varying(20) COLLATE pg_catalog."default",
    "Address" character varying(40) COLLATE pg_catalog."default",
    "Address2" character varying(40) COLLATE pg_catalog."default",
    "Province" character varying(20) COLLATE pg_catalog."default",
    "City" character varying(20) COLLATE pg_catalog."default",
    "ZipCode" character varying(6) COLLATE pg_catalog."default",
    "Avatar200x200" character varying(255) COLLATE pg_catalog."default",
    "BaseLocale" character varying(10) COLLATE pg_catalog."default" DEFAULT 'id-ID'::character varying,
    "BaseLanguage" character varying(10) COLLATE pg_catalog."default" DEFAULT 'id-ID'::character varying,
    "IsCashier" smallint DEFAULT 0,
    "IsRoot" smallint DEFAULT 1,
    "UserRole" character varying(20) COLLATE pg_catalog."default",
    "MustChangePassword" smallint DEFAULT 0,
    "RootUserName" character varying(32) COLLATE pg_catalog."default",
    "ParentUserName" character varying(32) COLLATE pg_catalog."default",
    "IsEmailConfirmed" smallint DEFAULT 0,
    "IsPhoneConfirmed" smallint DEFAULT 0,
    "IsActive" smallint DEFAULT 1,
    "LastVisitedURL" character varying(255) COLLATE pg_catalog."default",
    "LastLoginTime" timestamp without time zone,
    "LastLogoutTime" timestamp without time zone,
    "MembershipId" character varying(3) COLLATE pg_catalog."default" DEFAULT '000'::character varying,
    "TermsAgrement" smallint DEFAULT 1,
    "CreatedAt" timestamp without time zone DEFAULT now(),
    "UpdatedAt" timestamp without time zone,
    CONSTRAINT users_pkey PRIMARY KEY ("UserId")
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.users
    OWNER to postgre;


--
-- TOC entry 218 (class 1259 OID 16415)
-- Name: vUsers; Type: VIEW; Schema: public; Owner: postgre
--

CREATE VIEW public."vUsers" AS
 SELECT "UserId",
    "UserName",
    "Email",
    "FirstName",
    "LastName",
    "IsActive"
   FROM public.users
  ORDER BY "UserName";


ALTER VIEW public."vUsers" OWNER TO postgre;

--
-- TOC entry 3211 (class 2604 OID 16392)
-- Name: tbsystoken Id; Type: DEFAULT; Schema: public; Owner: postgre
--

ALTER TABLE ONLY public.tbsystoken ALTER COLUMN "Id" SET DEFAULT nextval('public."tbsystoken_Id_seq"'::regclass);


--
-- TOC entry 3381 (class 0 OID 0)
-- Dependencies: 215
-- Name: tbsystoken_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgre
--

SELECT pg_catalog.setval('public."tbsystoken_Id_seq"', 18, true);


--
-- TOC entry 3225 (class 2606 OID 16397)
-- Name: tbsystoken tbsystoken_pkey; Type: CONSTRAINT; Schema: public; Owner: postgre
--

ALTER TABLE ONLY public.tbsystoken
    ADD CONSTRAINT tbsystoken_pkey PRIMARY KEY ("Id");


--
-- TOC entry 3227 (class 2606 OID 16414)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgre
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY ("UserId");


-- Completed on 2025-06-17 07:16:37 UTC

--
-- PostgreSQL database dump complete
--

