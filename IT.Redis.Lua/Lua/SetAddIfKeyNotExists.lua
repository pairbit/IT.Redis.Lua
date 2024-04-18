if redis.call('exists', KEYS[1]) ~= 1 then
	return redis.call('sadd', KEYS[1], unpack(ARGV))
else
	return -1
end