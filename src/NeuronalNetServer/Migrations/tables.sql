create table traffic_sign (
	id int unsigned auto_increment,
	sign_type varchar(64) not null,
	bitmap mediumblob not null,
	uploaded datetime not null,
	primary key (id)
)