USE [motorbike]
GO
/****** Object:  Table [dbo].[account]    Script Date: 24/06/2025 1:49:44 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[account](
	[account_id] [int] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](50) NOT NULL,
	[password] [varchar](255) NOT NULL,
	[email] [nvarchar](100) NOT NULL,
	[phone] [nvarchar](20) NULL,
	[role_id] [int] NOT NULL,
	[address] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[account_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[contact]    Script Date: 24/06/2025 1:49:44 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[contact](
	[contact_id] [int] IDENTITY(1,1) NOT NULL,
	[account_id] [int] NULL,
	[name] [nvarchar](100) NOT NULL,
	[email] [nvarchar](100) NOT NULL,
	[phone] [nvarchar](20) NULL,
	[subject] [nvarchar](200) NOT NULL,
	[message] [nvarchar](max) NOT NULL,
	[created_at] [datetime] NOT NULL,
	[status] [nvarchar](20) NOT NULL,
	[response] [nvarchar](max) NULL,
	[response_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[contact_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[motorbike]    Script Date: 24/06/2025 1:49:44 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[motorbike](
	[motorbike_id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[brand_id] [int] NOT NULL,
	[price] [decimal](12, 2) NOT NULL,
	[image] [nvarchar](255) NULL,
	[total_sold] [int] NULL,
	[stock] [int] NOT NULL,
	[description] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[motorbike_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[motorbike_brand]    Script Date: 24/06/2025 1:49:44 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[motorbike_brand](
	[brand_id] [int] IDENTITY(1,1) NOT NULL,
	[brand_name] [nvarchar](100) NOT NULL,
	[logo] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[brand_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[order_details]    Script Date: 24/06/2025 1:49:44 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[order_details](
	[detail_id] [int] IDENTITY(1,1) NOT NULL,
	[order_id] [int] NOT NULL,
	[motorbike_id] [int] NOT NULL,
	[quantity] [int] NOT NULL,
	[unit_price] [decimal](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[detail_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[orders]    Script Date: 24/06/2025 1:49:44 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[orders](
	[order_id] [int] IDENTITY(1,1) NOT NULL,
	[order_date] [date] NOT NULL,
	[delivery_date] [date] NULL,
	[account_id] [int] NULL,
	[status] [nvarchar](50) NULL,
	[address] [nvarchar](255) NULL,
	[total_price] [decimal](12, 2) NULL,
	[payment_method] [nvarchar](50) NULL,
	[created_at] [datetime] NULL,
	[phone] [nvarchar](20) NULL,
	[customer_name] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[order_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[user_role]    Script Date: 24/06/2025 1:49:44 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[user_role](
	[role_id] [int] IDENTITY(1,1) NOT NULL,
	[role_name] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[role_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[account] ON 

INSERT [dbo].[account] ([account_id], [username], [password], [email], [phone], [role_id], [address]) VALUES (2, N'customer1', N'123456', N'customer1@gmail.com', N'0123456789', 2, N'Quận 4, Hồ Chí Minh')
INSERT [dbo].[account] ([account_id], [username], [password], [email], [phone], [role_id], [address]) VALUES (3, N'customer2', N'123456', N'customer2@gmail.com', N'0123456788', 2, N'Đà Nẵng')
INSERT [dbo].[account] ([account_id], [username], [password], [email], [phone], [role_id], [address]) VALUES (5, N'longpro03', N'XmOXJ5dG+t0GDYazHp3HlduIiN7i/Y6zPjFMpWaw200=', N'longkovipb52@gmail.com', N'0367547809', 2, N'Duc Hoa, Duc Phong, Bu Dang, Binh Phuoc')
INSERT [dbo].[account] ([account_id], [username], [password], [email], [phone], [role_id], [address]) VALUES (6, N'user1', N'XmOXJ5dG+t0GDYazHp3HlduIiN7i/Y6zPjFMpWaw200=', N'user1@gmail.com', N'0367547809', 2, N'Duc Hoa, Duc Phong, Bu Dang, Binh Phuoc')
INSERT [dbo].[account] ([account_id], [username], [password], [email], [phone], [role_id], [address]) VALUES (7, N'admin', N'6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=', N'admin@gmail.com', N'0123456789', 1, N'admin')
INSERT [dbo].[account] ([account_id], [username], [password], [email], [phone], [role_id], [address]) VALUES (9, N'user', N'XmOXJ5dG+t0GDYazHp3HlduIiN7i/Y6zPjFMpWaw200=', N'user@gmail.com', N'0123456789', 2, N'Helloppp')
INSERT [dbo].[account] ([account_id], [username], [password], [email], [phone], [role_id], [address]) VALUES (10, N'hihi', N'SnX+96yEYms6/C6qRk1kofJTOc+u4d1NoeZWot+QAPA=', N'hihi@gmail.com', N'0123456789', 2, N'Hello ')
SET IDENTITY_INSERT [dbo].[account] OFF
GO
SET IDENTITY_INSERT [dbo].[contact] ON 

INSERT [dbo].[contact] ([contact_id], [account_id], [name], [email], [phone], [subject], [message], [created_at], [status], [response], [response_at]) VALUES (1, 2, N'Nguyễn Văn A', N'nguyenvana@example.com', N'0123456789', N'Yêu cầu báo giá xe Honda SH', N'Tôi muốn biết giá chính xác và các chương trình khuyến mãi hiện có cho dòng xe Honda SH 150i mới nhất.', CAST(N'2025-06-19T22:02:33.400' AS DateTime), N'Đã xử lý', N'Chào anh/chị, chúng tôi đã gửi báo giá chi tiết qua email của anh/chị. Xin cảm ơn!', CAST(N'2025-06-20T00:02:33.400' AS DateTime))
INSERT [dbo].[contact] ([contact_id], [account_id], [name], [email], [phone], [subject], [message], [created_at], [status], [response], [response_at]) VALUES (4, NULL, N'NGUYEN VAN LONG', N'longkovipb52@gmail.com', N'0367547809', N'alokio', N'tôi cần hỗ trợ', CAST(N'2025-06-19T22:13:32.297' AS DateTime), N'Đã xử lý', N'Hỗ trợ gì bạn', CAST(N'2025-06-22T02:08:03.693' AS DateTime))
INSERT [dbo].[contact] ([contact_id], [account_id], [name], [email], [phone], [subject], [message], [created_at], [status], [response], [response_at]) VALUES (5, 5, N'longpro03', N'longkovipb52@gmail.com', N'0367547809', N'ronaldo ', N'messi', CAST(N'2025-06-19T22:14:08.407' AS DateTime), N'Đã xử lý', N'Oke messi', CAST(N'2025-06-22T02:01:16.743' AS DateTime))
INSERT [dbo].[contact] ([contact_id], [account_id], [name], [email], [phone], [subject], [message], [created_at], [status], [response], [response_at]) VALUES (7, 9, N'user', N'user@gmail.com', N'0123456789', N'Ducati ', N'hello', CAST(N'2025-06-22T02:34:46.953' AS DateTime), N'Đã xử lý', N'sssss', CAST(N'2025-06-22T03:13:16.800' AS DateTime))
SET IDENTITY_INSERT [dbo].[contact] OFF
GO
SET IDENTITY_INSERT [dbo].[motorbike] ON 

INSERT [dbo].[motorbike] ([motorbike_id], [name], [brand_id], [price], [image], [total_sold], [stock], [description]) VALUES (1, N'Honda Wave Alpha', 1, CAST(17800000.00 AS Decimal(12, 2)), N'ducati_panigaleV4.png', 151, 49, N'Honda Wave Alpha là mẫu xe số phổ thông nổi bật với thiết kế nhỏ gọn, bền bỉ và tiết kiệm nhiên liệu, phù hợp với nhu cầu di chuyển hằng ngày của người dùng Việt Nam. Với nhiều cải tiến về động cơ và kiểu dáng qua từng phiên bản, Wave Alpha luôn giữ vững vị thế là dòng xe số bán chạy hàng đầu trong phân khúc.

Đặc điểm nổi bật:
Thiết kế hiện đại, năng động: Xe sở hữu kiểu dáng thể thao, trẻ trung với các đường nét gọn gàng, phù hợp với nhiều đối tượng người dùng.

Động cơ mạnh mẽ, bền bỉ: Trang bị động cơ 110cc, 4 kỳ, làm mát bằng không khí, giúp vận hành ổn định và tăng tốc mượt mà.

Tiết kiệm nhiên liệu: Nhờ công nghệ tiên tiến của Honda, Wave Alpha có mức tiêu hao nhiên liệu ấn tượng, chỉ khoảng 1.8 lít/100km.

Khả năng vận hành linh hoạt: Trọng lượng nhẹ, dễ điều khiển, phù hợp cho cả nam và nữ, đặc biệt là khi đi trong thành phố đông đúc.

Chi phí bảo dưỡng thấp: Phụ tùng dễ tìm, chi phí sửa chữa hợp lý, là lựa chọn kinh tế lâu dài.

Thông số kỹ thuật cơ bản:
Dung tích xi lanh: 109.2cc

Công suất tối đa: 6.12 kW tại 7.500 vòng/phút

Dung tích bình xăng: 3.7 lít

Hệ thống khởi động: Điện và cần đạp

Trọng lượng: Khoảng 97 kg

Hệ thống truyền động: 4 số tròn')
INSERT [dbo].[motorbike] ([motorbike_id], [name], [brand_id], [price], [image], [total_sold], [stock], [description]) VALUES (2, N'Honda SH 150i', 1, CAST(102900000.00 AS Decimal(12, 2)), N'ducati_panigaleV4.png', 141, 9, N'Honda SH 150i là m?u xe tay ga cao c?p hàng d?u c?a Honda, n?i b?t v?i thi?t k? sang tr?ng, d?ng co m?nh m? và trang b? công ngh? hi?n d?i. Ðây là l?a ch?n lý tu?ng cho nh?ng khách hàng yêu thích s? d?ng c?p, ti?n nghi và tr?i nghi?m lái vu?t tr?i.')
INSERT [dbo].[motorbike] ([motorbike_id], [name], [brand_id], [price], [image], [total_sold], [stock], [description]) VALUES (3, N'Yamaha Exciter 155 VVA', 2, CAST(52900000.00 AS Decimal(12, 2)), N'ducati_panigaleV4.png', 209, 36, N'ducati_panigaleV4.png')
INSERT [dbo].[motorbike] ([motorbike_id], [name], [brand_id], [price], [image], [total_sold], [stock], [description]) VALUES (4, N'Yamaha Grande Hybrid', 2, CAST(52000000.00 AS Decimal(12, 2)), N'ducati_panigaleV4.png', 92, 23, N'ducati_panigaleV4.png')
INSERT [dbo].[motorbike] ([motorbike_id], [name], [brand_id], [price], [image], [total_sold], [stock], [description]) VALUES (5, N'Suzuki Raider R150 FI', 3, CAST(51990000.00 AS Decimal(12, 2)), N'ducati_panigaleV4.png', 86, 19, N'ducati_panigaleV4.png')
INSERT [dbo].[motorbike] ([motorbike_id], [name], [brand_id], [price], [image], [total_sold], [stock], [description]) VALUES (6, N'Kawasaki Ninja ZX-10R', 4, CAST(690000000.00 AS Decimal(12, 2)), N'ducati_panigaleV4.png', 16, 20, N'ducati_panigaleV4.png')
INSERT [dbo].[motorbike] ([motorbike_id], [name], [brand_id], [price], [image], [total_sold], [stock], [description]) VALUES (7, N'Ducati Panigale V4', 5, CAST(950000000.00 AS Decimal(12, 2)), N'ducati_panigaleV4.png', 20, 18, N'Ducati Panigale V4 là siêu mô tô th? thao v?i thi?t k? d?y tính khí d?ng h?c, s? h?u d?ng co V4 m?nh m? và nhi?u công ngh? dua xe, mang d?n c?m giác lái xe nhu trên du?ng dua.')
INSERT [dbo].[motorbike] ([motorbike_id], [name], [brand_id], [price], [image], [total_sold], [stock], [description]) VALUES (8, N'Honda Air Blade 150', 1, CAST(56990000.00 AS Decimal(12, 2)), N'ducati_panigaleV4.png', 137, 33, N'Honda Air Blade 150 v?i thi?t k? th? thao, góc c?nh cùng d?ng co 150cc m?nh m? và ti?t ki?m nhiên li?u, là s? l?a ch?n hàng d?u trong phân khúc xe tay ga th? thao t?m trung.')
SET IDENTITY_INSERT [dbo].[motorbike] OFF
GO
SET IDENTITY_INSERT [dbo].[motorbike_brand] ON 

INSERT [dbo].[motorbike_brand] ([brand_id], [brand_name], [logo]) VALUES (1, N'Honda', N'Honda_Logo.svg')
INSERT [dbo].[motorbike_brand] ([brand_id], [brand_name], [logo]) VALUES (2, N'Yamaha', N'Yamaha-Logo.png')
INSERT [dbo].[motorbike_brand] ([brand_id], [brand_name], [logo]) VALUES (3, N'Suzuki', N'Suzuki.png')
INSERT [dbo].[motorbike_brand] ([brand_id], [brand_name], [logo]) VALUES (4, N'Kawasaki', N'Kawasaki_Logo.png')
INSERT [dbo].[motorbike_brand] ([brand_id], [brand_name], [logo]) VALUES (5, N'Ducati', N'Ducati_Logo.png')
SET IDENTITY_INSERT [dbo].[motorbike_brand] OFF
GO
SET IDENTITY_INSERT [dbo].[order_details] ON 

INSERT [dbo].[order_details] ([detail_id], [order_id], [motorbike_id], [quantity], [unit_price]) VALUES (9, 11, 1, 1, CAST(17800000.00 AS Decimal(12, 2)))
INSERT [dbo].[order_details] ([detail_id], [order_id], [motorbike_id], [quantity], [unit_price]) VALUES (11, 13, 6, 2, CAST(690000000.00 AS Decimal(12, 2)))
INSERT [dbo].[order_details] ([detail_id], [order_id], [motorbike_id], [quantity], [unit_price]) VALUES (12, 14, 2, 1, CAST(102900000.00 AS Decimal(12, 2)))
INSERT [dbo].[order_details] ([detail_id], [order_id], [motorbike_id], [quantity], [unit_price]) VALUES (13, 15, 3, 6, CAST(52900000.00 AS Decimal(12, 2)))
INSERT [dbo].[order_details] ([detail_id], [order_id], [motorbike_id], [quantity], [unit_price]) VALUES (14, 16, 3, 1, CAST(52900000.00 AS Decimal(12, 2)))
INSERT [dbo].[order_details] ([detail_id], [order_id], [motorbike_id], [quantity], [unit_price]) VALUES (15, 17, 8, 3, CAST(56990000.00 AS Decimal(12, 2)))
INSERT [dbo].[order_details] ([detail_id], [order_id], [motorbike_id], [quantity], [unit_price]) VALUES (22, 24, 7, 1, CAST(950000000.00 AS Decimal(12, 2)))
INSERT [dbo].[order_details] ([detail_id], [order_id], [motorbike_id], [quantity], [unit_price]) VALUES (23, 25, 7, 1, CAST(950000000.00 AS Decimal(12, 2)))
SET IDENTITY_INSERT [dbo].[order_details] OFF
GO
SET IDENTITY_INSERT [dbo].[orders] ON 

INSERT [dbo].[orders] ([order_id], [order_date], [delivery_date], [account_id], [status], [address], [total_price], [payment_method], [created_at], [phone], [customer_name]) VALUES (11, CAST(N'2025-06-19' AS Date), CAST(N'2025-06-22' AS Date), 5, N'Chờ xác nhận', N'Duc Hoa, Duc Phong, Bu Dang, Binh Phuoc', CAST(17800000.00 AS Decimal(12, 2)), N'COD', CAST(N'2025-06-19T16:41:55.207' AS DateTime), N'0367547809', N'longpro03')
INSERT [dbo].[orders] ([order_id], [order_date], [delivery_date], [account_id], [status], [address], [total_price], [payment_method], [created_at], [phone], [customer_name]) VALUES (13, CAST(N'2025-06-19' AS Date), CAST(N'2025-06-22' AS Date), 5, N'Chờ xác nhận', N'Duc Hoa, Duc Phong, Bu Dang, Binh Phuoc', CAST(1380000000.00 AS Decimal(12, 2)), N'COD', CAST(N'2025-06-19T16:50:47.163' AS DateTime), N'0367547809', N'longpro03')
INSERT [dbo].[orders] ([order_id], [order_date], [delivery_date], [account_id], [status], [address], [total_price], [payment_method], [created_at], [phone], [customer_name]) VALUES (14, CAST(N'2025-06-19' AS Date), CAST(N'2025-06-22' AS Date), 5, N'Chờ xác nhận', N'Duc Hoa, Duc Phong, Bu Dang, Binh Phuoc', CAST(102900000.00 AS Decimal(12, 2)), N'COD', CAST(N'2025-06-19T16:56:40.780' AS DateTime), N'0367547809', N'longpro03')
INSERT [dbo].[orders] ([order_id], [order_date], [delivery_date], [account_id], [status], [address], [total_price], [payment_method], [created_at], [phone], [customer_name]) VALUES (15, CAST(N'2025-06-19' AS Date), CAST(N'2025-06-22' AS Date), 5, N'Chờ xác nhận', N'Duc Hoa, Duc Phong, Bu Dang, Binh Phuoc', CAST(317400000.00 AS Decimal(12, 2)), N'COD', CAST(N'2025-06-19T19:59:18.637' AS DateTime), N'0367547809', N'longpro03')
INSERT [dbo].[orders] ([order_id], [order_date], [delivery_date], [account_id], [status], [address], [total_price], [payment_method], [created_at], [phone], [customer_name]) VALUES (16, CAST(N'2025-06-20' AS Date), CAST(N'2025-06-23' AS Date), 6, N'Chờ xác nhận', N'Duc Hoa, Duc Phong, Bu Dang, Binh Phuoc', CAST(52900000.00 AS Decimal(12, 2)), N'BankTransfer', CAST(N'2025-06-20T07:55:57.297' AS DateTime), N'0367547809', N'user1')
INSERT [dbo].[orders] ([order_id], [order_date], [delivery_date], [account_id], [status], [address], [total_price], [payment_method], [created_at], [phone], [customer_name]) VALUES (17, CAST(N'2025-06-20' AS Date), CAST(N'2025-06-23' AS Date), 6, N'Chờ xác nhận', N'Duc Hoa, Duc Phong, Bu Dang, Binh Phuoc', CAST(170970000.00 AS Decimal(12, 2)), N'BankTransfer', CAST(N'2025-06-20T09:27:19.263' AS DateTime), N'0367547809', N'user1')
INSERT [dbo].[orders] ([order_id], [order_date], [delivery_date], [account_id], [status], [address], [total_price], [payment_method], [created_at], [phone], [customer_name]) VALUES (24, CAST(N'2025-06-22' AS Date), CAST(N'2025-06-25' AS Date), 9, N'Đã giao hàng', N'Helloppp', CAST(950000000.00 AS Decimal(12, 2)), N'COD', CAST(N'2025-06-22T02:34:09.673' AS DateTime), N'0123456789', N'user')
INSERT [dbo].[orders] ([order_id], [order_date], [delivery_date], [account_id], [status], [address], [total_price], [payment_method], [created_at], [phone], [customer_name]) VALUES (25, CAST(N'2025-06-23' AS Date), CAST(N'2025-06-26' AS Date), 10, N'Đã giao hàng', N'Hello', CAST(950000000.00 AS Decimal(12, 2)), N'COD', CAST(N'2025-06-23T16:47:22.610' AS DateTime), N'0123456789', N'hihi')
SET IDENTITY_INSERT [dbo].[orders] OFF
GO
SET IDENTITY_INSERT [dbo].[user_role] ON 

INSERT [dbo].[user_role] ([role_id], [role_name]) VALUES (1, N'admin')
INSERT [dbo].[user_role] ([role_id], [role_name]) VALUES (2, N'customer')
SET IDENTITY_INSERT [dbo].[user_role] OFF
GO
ALTER TABLE [dbo].[contact] ADD  CONSTRAINT [DF_contact_created_at]  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[contact] ADD  CONSTRAINT [DF_contact_status]  DEFAULT (N'Chưa xử lý') FOR [status]
GO
ALTER TABLE [dbo].[motorbike] ADD  DEFAULT ((0)) FOR [total_sold]
GO
ALTER TABLE [dbo].[motorbike] ADD  DEFAULT ((0)) FOR [stock]
GO
ALTER TABLE [dbo].[orders] ADD  CONSTRAINT [DF__orders__status__default]  DEFAULT (N'Chờ xác nhận') FOR [status]
GO
ALTER TABLE [dbo].[orders] ADD  CONSTRAINT [DF_orders_payment_method]  DEFAULT (N'COD') FOR [payment_method]
GO
ALTER TABLE [dbo].[orders] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[account]  WITH CHECK ADD FOREIGN KEY([role_id])
REFERENCES [dbo].[user_role] ([role_id])
GO
ALTER TABLE [dbo].[contact]  WITH CHECK ADD  CONSTRAINT [FK_contact_account] FOREIGN KEY([account_id])
REFERENCES [dbo].[account] ([account_id])
GO
ALTER TABLE [dbo].[contact] CHECK CONSTRAINT [FK_contact_account]
GO
ALTER TABLE [dbo].[motorbike]  WITH CHECK ADD FOREIGN KEY([brand_id])
REFERENCES [dbo].[motorbike_brand] ([brand_id])
GO
ALTER TABLE [dbo].[order_details]  WITH CHECK ADD FOREIGN KEY([motorbike_id])
REFERENCES [dbo].[motorbike] ([motorbike_id])
GO
ALTER TABLE [dbo].[order_details]  WITH CHECK ADD FOREIGN KEY([order_id])
REFERENCES [dbo].[orders] ([order_id])
GO
ALTER TABLE [dbo].[orders]  WITH CHECK ADD FOREIGN KEY([account_id])
REFERENCES [dbo].[account] ([account_id])
GO
