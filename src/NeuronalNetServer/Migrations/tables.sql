create table traffic_sign (
	id int unsigned auto_increment,
	sign_type varchar(64) not null,
	red_data blob not null,
	green_data blob not null,
	blue_data blob not null,
	uploaded datetime not null,
	primary key (id)
)