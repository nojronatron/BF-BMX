namespace BFBMX.Service.Test.TestData
{
    public static class SampleMessages
    {
        public static string ValidSingleMesageWithSevenBibRecords => @"Date: Sun, 13 Aug 2023 19:54:29 +0000
From: C4LL@winlink.org
Reply-To: C4LL@winlink.org
Subject: FW: Bigfoot 200 Chain of Lakes Message #2
To: C0LL@winlink.org
Message-ID: 0K3K2DET73LU
X-Cancel: 2023/09/03 19:54
X-Source: C4LL
X-Location: 46.145833N, 122.208333W (Grid square)
MIME-Version: 1.0
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundaryEegr9w==""

--boundaryEegr9w==
Content-Type: text/plain; charset=""iso-8859-1""
Content-Transfer-Encoding: quoted-printable


----- Message from C0LL was forwarded without change by C4LL at 2023-08-=
13 19:53 UTC -----

Message ID: PIY0NSXF2JJ2
Date: 2023/08/13 19:52
From: C4LL
To: C0LL=20
Source: C4LL
P2P: True
Location: 33.604167N, 117.291667W (Grid square)
Subject: Bigfoot 200 Chain of Lakes Message #2

-
130	IN	1149	13	CH
181	OUT	1149	13	CH
181	IN	1140	13	CH
249	IN	1143	13	CH
224	OUT	1149	13	CH
224	IN	1137	13	CH
96	OUT	1032	13	CH
-----

* The entries in this email are TAB delimited. This allows you to copy and =
paste into a spreadsheet.
-----
Winlink Express Sender C0LL=20
Template version 1.1.7


[No changes or editing of this message are allowed]

--boundaryEegr9w==--
";

        public static string NonconformingBibReportMessage => @"Date: Fri, 11 Aug 2023 21:25:00 +0000
From: C4LL@winlink.org
Subject: list 9
To: C0LL@winlink.org
Message-ID: 3HPR0R1L20LD
X-Source: C4LL
X-P2P: True
X-Location: 46.145833N, 122.291667W (Grid square)
MIME-Version: 1.0
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundaryS/HRkA==""

--boundaryS/HRkA==
Content-Type: text/plain; charset=""iso-8859-1""
Content-Transfer-Encoding: quoted-printable

104
184
109
58
--boundaryS/HRkA==--
";

        public static string RepliedToWinlinkMessageNoBibData => @"Date: Fri, 11 Aug 2023 20:15:00 +0000
From: C4LL@winlink.org
Subject: Re: Marble Mountain Test
To: C0LL@winlink.org
Message-ID: 2QH411ZD77CZ
X-Source: C4LL
X-Location: 46.479167N, 121.791667W (GRID SQUARE)
MIME-Version: 1.0
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundaryMruAaw==""

--boundaryMruAaw==
Content-Type: text/plain; charset=""iso-8859-1""
Content-Transfer-Encoding: quoted-printable

winlink works. we had negative copy on 40 or 80, not even a hint of a trans=
mission,



----- Message from C0LL sent 2023/08/11 19:45 -----

Message ID: JRDOQHOTYEFC
Date: 2023/08/11 19:45
From: C4LL
To: C0LL; H4LL=20
Source: C4LL
Location: 46.130545N, 122.171410W (GPS)
Subject: Marble Mountain Test

Hello Windy Ridge from Marble Mountain! NO VOICE via 40m nor 80m. Will Winl=
ink be goo enough? Let us know how to proceed to ensure we have a link for =
the duration of te 40 mile event.

Jon
--boundaryMruAaw==--
";

        public static string ValidMessageWithBibDataInReplyMessage => @"Date: Sun, 13 Aug 2023 19:54:29 +0000
From: C4LL@winlink.org
Reply-To: C4LL@winlink.org
Subject: FW: Bigfoot 200 Chain of Lakes Message #2
To: C0LL@winlink.org
Message-ID: 0K3K2DET73LU
X-Cancel: 2023/09/03 19:54
X-Source: C4LL
X-Location: 46.145833N, 122.208333W (Grid square)
MIME-Version: 1.0
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundaryEegr9w==""

--boundaryEegr9w==
Content-Type: text/plain; charset=""iso-8859-1""
Content-Transfer-Encoding: quoted-printable


----- Message from X0LL was forwarded without change by C4LL at 2023-08-13 19:53 UTC -----

Message ID: PIY0NSXF2JJ2
Date: 2023/08/13 19:52
From: X0LL
To: C4LL=20
Source: X0LL
P2P: True
Location: 33.604167N, 117.291667W (Grid square)
Subject: Bigfoot 200 Chain of Lakes Message #2

-
130	IN	1149	13	CH
181	OUT	1149	13	CH
181	IN	1140	13	CH
249	IN	1143	13	CH
224	OUT	1149	13	CH
224	IN	1137	13	CH
96	OUT	1032	13	CH
-----

* The entries in this email are TAB delimited. This allows you to copy and =
paste into a spreadsheet.
-----
Winlink Express Sender X0LL=20
Template version 1.1.7


[No changes or editing of this message are allowed]

--boundaryEegr9w==--
";

        public static string MessageWith26ValidBibs => @"Date: Mon, 14 Aug 2023 01:25:57 +0000
From: C4LL@winlink.org
Reply-To: C4LL@winlink.org
Subject: Bigfoot 200 2023 Chain of Lakes Message #4
To: C0LL@winlink.org
Message-ID: 0LI1KQMC3XVP
X-Cancel: 2023/09/04 01:25
X-Source: C4LL
X-Location: 46.293940N, 121.594845W (GPS)
MIME-Version: 1.0
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundaryOTJvbw==""

--boundaryOTJvbw==
Content-Type: text/plain; charset=""iso-8859-1""
Content-Transfer-Encoding: quoted-printable

-
210	IN	1715	13	CH
177	OUT	1649	13	CH
132	OUT	1649	13	CH
132	IN	1640	13	CH
86	OUT	1637	13	CH
174	OUT	1632	13	CH
134	OUT	1632	13	CH
190	OUT	1623	13	CH
102	OUT	1612	13	CH
145	OUT	1620	13	CH
225	OUT	1609	13	CH
67	OUT	1555	13	CH
49	OUT	1550	13	CH
137	OUT	1539	13	CH
145	IN	1534	13	CH
190	IN	1534	13	CH
225	IN	1529	13	CH
100	IN	1525	13	CH
174	IN	1525	13	CH
134	IN	1525	13	CH
117	IN	1522	13	CH
64	OUT	1457	13	CH
106	OUT	1453	13	CH
86	IN	1451	13	CH
49	IN	1448	13	CH
209	IN	1443	13	CH
-----

* The entries in this email are TAB delimited. This allows you to copy and =
paste into a spreadsheet.
-----
Winlink Express Sender C4LL=20
Template version 1.1.7

--boundaryOTJvbw==--
";

        public static string ValidMessageWith5SpaceDelimitedBibs => @"
        public static string MessageWith26ValidBibs => @""Date: Mon, 14 Aug 2023 01:25:57 +0000
From: C4LL@winlink.org
Reply-To: C4LL@winlink.org
Subject: Bigfoot 200 2023 Chain of Lakes Message #4
To: C0LL@winlink.org
Message-ID: 0LI1KQMC3XVP
X-Cancel: 2023/09/04 01:25
X-Source: C4LL
X-Location: 46.293940N, 121.594845W (GPS)
MIME-Version: 1.0
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""""boundaryOTJvbw==""""

--boundaryOTJvbw==
Content-Type: text/plain; charset=""""iso-8859-1""""
Content-Transfer-Encoding: quoted-printable

-

64 OUT 1457 13 CH
106 OUT 1453 13 CH
86 IN 1451 13 CH
49 IN 1448 13 CH
209 IN 1443 13 CH
-----

* The entries in this email are TAB delimited. This allows you to copy and =
paste into a spreadsheet.
-----
Winlink Express Sender C4LL=20
Template version 1.1.7

--boundaryOTJvbw==--
"";
";

        public static string ValidMessageWithCommaDelimintedBibs => @"        public static string ValidMessageWith5SpaceDelimitedBibs => @""
        public static string MessageWith26ValidBibs => @""""Date: Mon, 14 Aug 2023 01:25:57 +0000
From: C4LL@winlink.org
Reply-To: C4LL@winlink.org
Subject: Bigfoot 200 2023 Chain of Lakes Message #4
To: C0LL@winlink.org
Message-ID: 0LI1KQMC3XVP
X-Cancel: 2023/09/04 01:25
X-Source: C4LL
X-Location: 46.293940N, 121.594845W (GPS)
MIME-Version: 1.0
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""""""""boundaryOTJvbw==""""""""

--boundaryOTJvbw==
Content-Type: text/plain; charset=""""""""iso-8859-1""""""""
Content-Transfer-Encoding: quoted-printable

-

64, OUT ,1457,13 , CH
106 , OUT , 1453 , 13 , CH
86 ,IN ,1451 ,13 ,CH
49, IN, 1448, 13, CH
209,IN,1443,13,CH
-----

* The entries in this email are TAB delimited. This allows you to copy and =
paste into a spreadsheet.
-----
Winlink Express Sender C4LL=20
Template version 1.1.7

--boundaryOTJvbw==--
"""";
"";
";
    }
}
